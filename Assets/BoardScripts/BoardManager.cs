using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour {

    private enum GameStep {
        WaitingInput,
        CheckingInput,
        PlayingEffects,
        OrientingElements,
        GeneratingElements,
        CheckingBoardForMatches,
        MarkingPossibleInputs
    }

    private GameStep currentStep = GameStep.WaitingInput;

    [SerializeField]
    private float swappingSpeed = 3.0f;
    [SerializeField]
    int totalCollums = 4;
    [SerializeField]
    int totalRows = 4;
    [SerializeField]
    int maxScoreAllowed = 25;
    [SerializeField]
    public Color highlightColor;
    [SerializeField]
    Transform cellsLocations = null;
    [SerializeField]
    Vector3[] posOnWorld;

#region Panels
    /// <summary> this.transform</summary>
    Transform boardPanel = null;
    /// <summary> holds score and other UI</summary>
    Transform detailsPanel = null;
    /// <summary> Holds cells</summary>
    Transform gamePanel = null;
#endregion

    /// <summary> The elements of the board. 0 dimension is collums, first dimension is rows</summary>
    public BoardElement[, ] elementsPositions = null;
    /// <summary> Flagged elements that create matches</summary>
    public bool[, ] matchedElemPositions = null;
    /// <summary> Flagged destroyed elements, that need replacing</summary>
    public bool[, ] positionsDestroyed = null;
    public Vector3[, ] positionsOnWorld;

#region  Utility and holders
    /// <summary> Helping transforms to create the drop effect</summary>
    public RectTransform[] holders = null;
    [SerializeField]
    RectTransform holdersParent = null;

    /// <summary> Value holder for step1 to step2</summary>
    BoardElement firstElement = null;
    /// <summary> Value holder for step1 to step2</summary>
    BoardElement secondElement = null;
#endregion

    // TO-DO: Create a proper singleton

    /// <summary> The instance of the manager</summary>
    public static BoardManager inst;

    /// <summary>Animations currently playing / qued to play. At server-client relationship this will be send to client to be played</summary>
    public List<AnimationMessage> animationMessages = new List<AnimationMessage>();

    /// <summary>Audio clips currently playing / qued to play. At server-client relationship this will be send to client to be played</summary>
    public List<AudioMessage> audioMessages = new List<AudioMessage>();
    /// <summary> should be set false by the client</summary>
    public static bool areAnimationsPlaying = false;

    GameClient attachedClient = null;

    void Awake() {
        inst = this;
        boardPanel = this.transform;
        detailsPanel = boardPanel.GetChild(0);
        gamePanel = GameObject.Find("GamePanel").transform; //boardPanel.GetChild(1);

        elementsPositions = new BoardElement[totalCollums, totalRows];
        positionsOnWorld = new Vector3[totalCollums, totalRows];
        matchedElemPositions = new bool[totalCollums, totalRows];
        positionsDestroyed = new bool[totalCollums, totalRows];
        holders = new RectTransform[holdersParent.childCount];
        for (int i = 0; i < holders.Length; i++) {
            holders[i] = holdersParent.GetChild(i).GetComponent<RectTransform>();
        }
        // Debug
        //DebugCheckPanels();

        int rowIndex = 0;
        int collumIndex = 0;

        // Assign elements to the board

        // foreach gameobject child of gameobject "GamePanel"
        for (int i = 0; i < gamePanel.childCount; i++) {
            // if in set bounds
            if (collumIndex < totalCollums && rowIndex < totalRows) {
                posOnWorld = new Vector3[gamePanel.childCount];
                // Get a random color
                int newColorIndex = Random.Range(0, BoardFunctions.GetAvailableColors().Length);
                Color newColor = BoardFunctions.GetAvailableColors() [newColorIndex];

                // Create a new gameobject that will hold Image component for this element
                GameObject imageChild = new GameObject("imageChild");
                imageChild.AddComponent<Image>();
                imageChild.GetComponent<Image>().sprite = AssetLoader.GetDefaultElementSprite();
                imageChild.GetComponent<Image>().preserveAspect = true;

                // set parent of the new gameobject to current child proccessed
                imageChild.transform.parent = gamePanel.GetChild(i);
                imageChild.transform.localScale = Vector3.one;
                imageChild.transform.localPosition = Vector3.zero;
                //imageChild.transform.position = Vector3.zero;
                // Set it above the gameobject that holds the "Highlight" Image component
                imageChild.transform.SetAsFirstSibling();
                // Create a new BoardElement instance an assign it to positions board
                elementsPositions[collumIndex, rowIndex] = new BoardElement(gamePanel.GetChild(i).gameObject, i, newColor);
                // new animation to show the default sprite on gameobject

                //Debug.Log(positions[collumIndex, rowIndex].name + " is at position: c" + collumIndex + ", r" + rowIndex);
                // gamePanel.GetChild(i).SetParent(null);

                // elementsPositions[collumIndex, rowIndex].GetAttachedGameObject().transform.SetParent(gamePanel, true);
                animationMessages.Add(new AnimationMessage(i, AssetLoader.GetDefaultElementSprite(), newColor));
                collumIndex++;
                // fix collum index
                if (collumIndex == totalCollums) {
                    rowIndex++;
                    collumIndex = 0;
                }
            }
            else {
                Debug.LogError("There are more transforms on the board than collums * rows");
            }

        }
        SendAnimationMessagesToClient(ref animationMessages);
    }
    IEnumerator CoWaitForPosition() {
        yield return new WaitForEndOfFrame();
        int i = 0;
        for (int row = 0; row < elementsPositions.GetLength(1); row++) {
            for (int collum = 0; collum < elementsPositions.GetLength(0); collum++) {
                positionsOnWorld[collum, row] = elementsPositions[collum, row].GetAttachedGameObject().GetComponent<RectTransform>().anchoredPosition;
                posOnWorld[i] = positionsOnWorld[collum, row];
                i++;
            }
        }
    }

    // Start is called before the first frame update
    void Start() {
        // Assign color values to elemets
        //int currentScore = 0;

        bool hasPotentialInputs = false;
        for (int row = 0; row < elementsPositions.GetLength(1); row++) {
            for (int collum = 0; collum < elementsPositions.GetLength(0); collum++) {
                if (BoardFunctions.IsPotentialInput(collum, row, elementsPositions, matchedElemPositions, totalCollums, totalRows) > 0 && !hasPotentialInputs) {
                    hasPotentialInputs = true;
                }
            }
        }
        StartCoroutine("CoWaitForPosition");
        currentStep = GameStep.CheckingBoardForMatches;
    }

    // Update is called once per frame
    void Update() {
        // Do nothing.
        if (areAnimationsPlaying) {
            return;
        }
        switch (currentStep) {
            case GameStep.WaitingInput:
                //OnDragEvent.hasDragBegin = false;
                break;

                // Step1: check if input create matches and if true move to step2
            case GameStep.CheckingInput:
                AlexDebugger.GetInstance().AddMessage("Entering Step1: -check if input between-" + firstElement.GetAttachedGameObject().transform.name + " and " + secondElement.GetAttachedGameObject().transform.name + " create matches", AlexDebugger.tags.Step1);
                //Debug.Log("Checking for matches based on input");

                // check if input create matches
                if (HandleInputForElement(firstElement, secondElement)) {
                    HandleInputForElement(secondElement, firstElement);
                    AlexDebugger.GetInstance().AddMessage("Step1 finished with matches, going to Step2: -play effects for matched elements-, input1:" + firstElement.GetAttachedGameObject().transform.name + ", input2: " + secondElement.GetAttachedGameObject().transform.name, AlexDebugger.tags.Step1);
                    currentStep = GameStep.PlayingEffects;
                }
                else if (HandleInputForElement(secondElement, firstElement)) {
                    AlexDebugger.GetInstance().AddMessage("Step1 finished with matches, going to Step2: -play effects for matched elements-, input1:" + firstElement.GetAttachedGameObject().transform.name + ", input2: " + secondElement.GetAttachedGameObject().transform.name, AlexDebugger.tags.Step1);
                    currentStep = GameStep.PlayingEffects;
                }
                else {
                    AlexDebugger.GetInstance().AddMessage("No matches found, when finish, go back to step0: -waiting for input-", AlexDebugger.tags.Step1);
                    currentStep = GameStep.WaitingInput;
                }
                firstElement = null;
                secondElement = null;
                break;
                // Step2: -Play effects for matched elements- and move to step3
            case GameStep.PlayingEffects:
                AlexDebugger.GetInstance().AddMessage("Entering Step2: -play effects for matched elements-", AlexDebugger.tags.Step2);
                if (PlayEffectsStep()) {
                    // AlexDebugger.GetInstance().AddMessage("Sending total animations to client: " + playingAnimations.Count, AlexDebugger.tags.Step2);
                    SendAnimationMessagesToClient(ref animationMessages);
                    currentStep = GameStep.OrientingElements;
                }
                break;

                // Step3: repeat until non-destroyed elements have droped to lower position and then move to step4
            case GameStep.OrientingElements:
                AlexDebugger.GetInstance().AddMessage("Entering Step3: -reorienting board-", AlexDebugger.tags.Step3);
                ReorientElements();
                SendAnimationMessagesToClient(ref animationMessages);
                //AlexDebugger.GetInstance().AddMessage("Step3: -reorienting board- has finished, moving to Step4: -replace destroyed elements", AlexDebugger.tags.Step3);
                currentStep = GameStep.GeneratingElements;
                break;
                // Step4: -Replace destroyed elements-, by assigning new color,  move to step5
            case GameStep.GeneratingElements:
                //AlexDebugger.GetInstance().AddMessage("Entering Step4: -replace destroyed elements-", AlexDebugger.tags.Step4);
                GenerateNewElemetns();
                //AlexDebugger.GetInstance().AddMessage("Sending total animations to client: " + playingAnimations.Count, AlexDebugger.tags.Step4);
                SendAnimationMessagesToClient(ref animationMessages);
                // AlexDebugger.GetInstance().AddMessage("Element: " + positions[bestInputCollum, bestInputRow].GetAttachedGameObject().name + " has the best score possible: " + maxOutput, AlexDebugger.tags.Step4);
                //AlexDebugger.GetInstance().AddMessage("Step4 -Replace destroyed elements- has finished, moving to Step5 -Aftermatch-", AlexDebugger.tags.Step4);
                currentStep = GameStep.CheckingBoardForMatches;
                break;
                // Step5: check for new matches
            case GameStep.CheckingBoardForMatches:
                AlexDebugger.GetInstance().AddMessage("Entering Step5: -Aftermatch-", AlexDebugger.tags.Step5);
                //Debug.Log("### AfterMatch ### Checking board for combos");
                int newTotalMatches = BoardFunctions.CheckBoardForMatches(elementsPositions, ref matchedElemPositions, totalCollums, totalRows);
                // Go back to step2
                if (newTotalMatches > 0) {
                    //Debug.Log("### AfterMatch ### Total new matches found: " + newTotalMatches);
                    AlexDebugger.GetInstance().AddMessage("Total new matches found on board: " + newTotalMatches, AlexDebugger.tags.Step5);
                    AlexDebugger.GetInstance().AddMessage("Step5 -Aftermatch- has finished, with state -new matches found-,  moving to Step2 -Play effects for matched elements-", AlexDebugger.tags.Step5);
                    currentStep = GameStep.PlayingEffects;
                }
                // Check for possible inputs
                else {
                    AlexDebugger.GetInstance().AddMessage("No new matches found, flagging potential input", AlexDebugger.tags.Step5);
                    currentStep = GameStep.MarkingPossibleInputs;
                }
                SendAnimationMessagesToClient(ref animationMessages);
                break;
            case GameStep.MarkingPossibleInputs:
                //Debug.Log("### AfterMatch ### No new matches found");
                if (!CheckForPossibleInputs()) {
                    //Debug.Log("### AfterMatch ### No possible inputs found, re-generating board");
                    AlexDebugger.GetInstance().AddMessage("No potential input found, all elements are flagged as destroyed", AlexDebugger.tags.Step5);
                    MarkAllElementsAsDestroyed();
                    AlexDebugger.GetInstance().AddMessage("Step5 -Aftermatch- has finished, with state -No available input-,  moving to Step4 -Replace destroyed elements-", AlexDebugger.tags.Step5);
                    currentStep = GameStep.GeneratingElements;
                }
                else {
                    AlexDebugger.GetInstance().AddMessage("Step5 -Aftermatch- has finished, with state -Has available input-,  moving to Step0 -Wait for input-", AlexDebugger.tags.Step5);
                    currentStep = GameStep.WaitingInput;
                }
                SendAnimationMessagesToClient(ref animationMessages);
                break;
        }
    }

    public static BoardManager GetInstance() {
        return inst;
    }

    public static BoardManager GetNewBoardManager() {
        BoardManager newBoard = GetInstance().transform.gameObject.AddComponent<BoardManager>();
        return newBoard;
    }

    /// <summary>Receives input from client and handles it</summary>
    public void TakeInput(int firstElementIndexAtParent, int secondElementIndexAtParent) {
        // find first element based on gamePanel's child index
        firstElement = BoardFunctions.GetElementBasedOnParentIndex(elementsPositions, firstElementIndexAtParent);
        // find second element based on gamePanel's child index
        secondElement = BoardFunctions.GetElementBasedOnParentIndex(elementsPositions, secondElementIndexAtParent);

        // Check if elements are neighbours thus input correct
        // TO-DO: make it to check if it is a possible mathch
        if (BoardFunctions.GetIfNeighbours(firstElement, secondElement, elementsPositions)) {
            AlexDebugger.GetInstance().AddMessage("Correct input: " + firstElement.GetAttachedGameObject().transform.name + ", with " + secondElement.GetAttachedGameObject().transform.name, AlexDebugger.tags.Input);
            // Remove tokens
            MoneyManager.ChangeBalanceBy(-MoneyManager.GetSwapCost());
            // Swap the elements on the board
            BoardFunctions.SwapElements(firstElement, secondElement, this, rewire : false);
            // Allow Update() to check if matches are created
            currentStep = GameStep.CheckingInput;
        }
        else {
            // Swap elements, on rewire mode
            BoardFunctions.SwapElements(firstElement, secondElement, this, rewire : true);
        }
        // Send animations to client and empty list
        SendAnimationMessagesToClient(ref animationMessages);
    }

    private bool PlayEffectsStep() {
        BoardElement lastElementProcessed = null;
        bool areThereChangesOnBoard = false;
        for (int row = 0; row < elementsPositions.GetLength(1); row++) {
            for (int collum = 0; collum < elementsPositions.GetLength(0); collum++) {
                if (matchedElemPositions[collum, row] == true) {
                    if (BoardFunctions.DestroyBoardElement(collum, row, ref elementsPositions, ref matchedElemPositions, ref animationMessages, lastElementProcessed)) {
                        areThereChangesOnBoard = true;
                    }
                    BoardFunctions.PlayMatchEffectAnimations(collum, row, elementsPositions, ref animationMessages, swappingSpeed);
                }
                lastElementProcessed = elementsPositions[collum, row];
                BoardFunctions.ToggleHighlightCell(collum, row, elementsPositions, false, highlightColor);
            }
        }

        if (!areThereChangesOnBoard) {
            AlexDebugger.GetInstance().AddMessage("Step2 finished: -play effects for matched elements-, go to Step3: -reorient elements- " + animationMessages.Count, AlexDebugger.tags.Step2);
            return true;
        }
        else {
            AlexDebugger.GetInstance().AddMessage("Step2 finished: -play effects for matched elements-, new elements has been destroyed, repeating step ", AlexDebugger.tags.Step2);
            return false;
        }
    }

    private void ReorientElements() {
        bool hasSFXplayed = false;
        for (int collum = 0; collum < totalCollums; collum++) {
            //AlexDebugger.GetInstance().AddMessage("Moving element: " + elementsPositions[collum, row].GetAttachedGameObject().transform.name + " upwards", AlexDebugger.tags.Step3);
            if (BoardFunctions.MoveMatchedElementUpwards(collum, totalRows - 1, this) && !hasSFXplayed) {
                audioMessages.Add(new AudioMessage(AssetLoader.GetCellDropSFX(), audioMessages.Count, -1, 0.0f, Random.Range(0.0f, 2.0f)));
                hasSFXplayed = true;
            }

        }
    }

    private void GenerateNewElemetns() {
        audioMessages.Add(new AudioMessage(AssetLoader.GetCellDropSFX(), audioMessages.Count, -1, 0.15f, Random.Range(0.0f, 2.0f)));
        bool[, ] searchedElements = new bool[positionsDestroyed.GetLength(0), positionsDestroyed.GetLength(1)];
        for (int row = 0; row < elementsPositions.GetLength(1); row++) {
            for (int collum = 0; collum < elementsPositions.GetLength(0); collum++) {
                if (positionsDestroyed[collum, row] == true) {
                    BoardFunctions.ReplaceElementAnimations(collum, row, this, ref searchedElements);
                    positionsDestroyed[collum, row] = false;

                }
            }
        }
    }
    private bool CheckForPossibleInputs() {
        bool hasPossibleInput = false;
        for (int row = 0; row < elementsPositions.GetLength(1); row++) {
            for (int collum = 0; collum < elementsPositions.GetLength(0); collum++) {
                if (BoardFunctions.IsPotentialInput(collum, row, elementsPositions, matchedElemPositions, totalCollums, totalRows) > 0) {
                    //
                    AlexDebugger.GetInstance().AddMessage(elementsPositions[collum, row].GetAttachedGameObject() + " is potential input", AlexDebugger.tags.Step5);
                    BoardFunctions.ToggleHighlightCell(collum, row, elementsPositions, true, highlightColor);
                    hasPossibleInput = true;
                }
                else {
                    BoardFunctions.ToggleHighlightCell(collum, row, elementsPositions, false, highlightColor);
                }
            }
        }
        return hasPossibleInput;
    }
    private void MarkAllElementsAsDestroyed() {
        for (int row = 0; row < elementsPositions.GetLength(1); row++) {
            for (int collum = 0; collum < elementsPositions.GetLength(0); collum++) {
                positionsDestroyed[collum, row] = true;
            }
        }
    }
    public bool
    HandleInputForElement(BoardElement element, BoardElement otherElement) {
        bool areThereMatches = false;
        if (element.GetElementClassType() == typeof(BombBoardElement)) {
            if (otherElement.GetElementClassType() == typeof(CrossBoardElement)) {
                AlexDebugger.GetInstance().AddMessage(element.GetAttachedGameObject() + " bomb style set to cross", AlexDebugger.tags.Step1);
                ((BombBoardElement) element).SetExplosionStyleTo(BombBoardElement.BombExplosionStyle.CrossStyle);
            }
            else if (otherElement.GetElementClassType() == typeof(BombBoardElement)) {
                AlexDebugger.GetInstance().AddMessage(element.GetAttachedGameObject() + " bomb style set to double bomb", AlexDebugger.tags.Step1);
                ((BombBoardElement) element).SetExplosionStyleTo(BombBoardElement.BombExplosionStyle.DoubleBombStyle);
            }
            element.OnElementDestruction(elementsPositions, ref matchedElemPositions);
            AlexDebugger.GetInstance().AddMessage("first element was a bomb, go to step2: -play effects for matched elements-", AlexDebugger.tags.Step1);
            areThereMatches = true;
        }
        else if (element.GetElementClassType() == typeof(BellBoardElement)) {
            AlexDebugger.GetInstance().AddMessage("first element was a bell, go to step2: -play effects for matched elements-", AlexDebugger.tags.Step1);
            element.OnElementDestruction(ref elementsPositions, ref matchedElemPositions, ref animationMessages, otherElement);
            areThereMatches = true;
        }
        else {
            KeyValuePair<int, int> firstPosition = BoardFunctions.GetBoardPositionOfElement(element, elementsPositions);
            int numberOfMatchesForFirst = BoardFunctions.CheckElementForMatches(firstPosition.Key, firstPosition.Value, elementsPositions, ref matchedElemPositions, totalCollums, totalRows, true);
            // if there are matches
            if (numberOfMatchesForFirst > 0) {
                AlexDebugger.GetInstance().AddMessage("Matches found for first element " + numberOfMatchesForFirst + ", go to step2: -play effects for matched elements-", AlexDebugger.tags.Step1);
                // allow Update() to play effects
                areThereMatches = true;
            }
        }
        return areThereMatches;
    }

    public void SendAnimationMessagesToClient(ref List<AnimationMessage> animMsg) {
        if (animMsg.Count > 0) {
            Animations.SetAnimationMessages(animMsg.ToArray());
            animMsg.Clear();
            areAnimationsPlaying = true;
        }
        if (audioMessages.Count > 0) {
            AudioManager.ReceiveAudioMessages(audioMessages);
            audioMessages.Clear();

        }
    }

    /// <summary>Is boardManager available</summary>
    public bool IsAvailable() {
        if (currentStep == GameStep.WaitingInput && !areAnimationsPlaying) {
            return true;
        }
        else {
            return false;
        }
    }

    public GameClient GetClientAttached() {
        return attachedClient;
    }
    public void SetClient(GameClient clientToAttach) {
        if (attachedClient == null) {
            attachedClient = clientToAttach;
        }
        else {
            Debug.LogWarning("Trying to attach client but client has already been attached");
        }
    }

    public float GetSwappingSpeed() {
        return swappingSpeed;
    }

    /// Debug
    private void DebugElementsMatches(List<BoardElement> elements) {
        if (elements.Count > 0) {
            string d = "";
            foreach (BoardElement e in elements) {
                d += e.GetAttachedGameObject().name + ", ";
            }
            AlexDebugger.GetInstance().AddMessage(d, AlexDebugger.tags.Matches);
            Debug.Log(d + ", Matches");
        }
    }
    private void DebugCheckPanels() {
#if UNITY_EDITOR
        if (detailsPanel.gameObject.name != "DetailsPanel") {
            Debug.LogError(boardPanel.gameObject.name + ".GetChild(0).name != DetailsPanel");
        }
        if (gamePanel.gameObject.name != "GamePanel") {
            Debug.LogError(boardPanel.gameObject.name + ".GetChild(1).name != GamePanel");
        }
        else {
            for (int i = 0; i < gamePanel.childCount; i++) {
                if (gamePanel.GetChild(i).GetComponent<UnityEngine.UI.Image>() == null) {
                    Debug.LogError(gamePanel.GetChild(i).name + " has no Image component");
                    gamePanel.GetChild(i).gameObject.AddComponent<UnityEngine.UI.Image>();
                }

            }
        }

#endif
    }

}