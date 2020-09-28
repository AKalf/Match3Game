using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour {
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

#region Panels
    /// <summary> this.transform</summary>
    Transform boardPanel = null;
    /// <summary> holds score and other UI</summary>
    Transform detailsPanel = null;
    /// <summary> Holds cells</summary>
    Transform gamePanel = null;
#endregion

    /// <summary> The elements of the board. 0 dimension is collums, first dimension is rows</summary>
    BoardElement[, ] positions = null;
    /// <summary> Flagged elements that create matches</summary>
    bool[, ] matchedElemPositions = null;
    /// <summary> Flagged destroyed elements, that need replacing</summary>
    bool[, ] positionsDestroyed = null;

#region  Utility and holders
    /// <summary> Helping transforms to create the drop effect</summary>
    Transform[] holders = null;
    [SerializeField]
    Transform holdersParent = null;

    /// <summary> Value holder for step1 to step2</summary>
    BoardElement firstElement = null;
    /// <summary> Value holder for step1 to step2</summary>
    BoardElement secondElement = null;
#endregion

    // TO-DO: Create a proper singleton

    /// <summary> The instance of the manager</summary>
    public static BoardManager inst;

    /// <summary>Animations currently playing / qued to play. At server-client relationship this will be send to client to be played</summary>
    private List<AnimationMessage> playingAnimations = new List<AnimationMessage>();

    /// <summary> should be set false by the client</summary>
    public static bool areAnimationsPlaying = false;

#region  GameLoop states
    /// <summary> Prevents input overwriting</summary>
    private bool areCellsSwapping = false;
    /// <summary> Step1 in Update, handle input</summary>
    bool shouldCheckIfInputCreatesMatches = false;
    /// <summary> Step2 if there are matches play effects</summary>
    bool shouldPlayEffects = false;
    /// <summary> Step3 re-orient board by moving non-destroyed elements donwards</summary>
    bool areElemetsReorienting = false;
    /// <summary> Step4 move any destroyed elemets to holders position</summary>
    bool shouldMoveElemToHolders = false;
    /// <summary> Step5 re-create elemets and move them to their position</summary>
    bool shouldGenerateElem = false;
    /// <summary> Step6 check for new matches at the board</summary>
    bool shouldCheckBoard = false;
#endregion

    GameClient attachedClient = null;

    void Awake() {
        inst = this;
        boardPanel = this.transform;
        detailsPanel = boardPanel.GetChild(0);
        gamePanel = boardPanel.GetChild(1);

        positions = new BoardElement[totalCollums, totalRows];
        matchedElemPositions = new bool[totalCollums, totalRows];
        positionsDestroyed = new bool[totalCollums, totalRows];
        holders = new Transform[holdersParent.childCount];

        for (int i = 0; i < holders.Length; i++) {
            holders[i] = holdersParent.GetChild(i);
        }
        // Debug
        //DebugCheckPanels();

        int rowIndex = 0;
        int collumIndex = 0;

        // Assign elements to the board
        for (int i = 0; i < gamePanel.childCount; i++) {
            if (collumIndex < totalCollums && rowIndex < totalRows) {
                int newColorIndex = Random.Range(0, BoardFunctions.GetAvailableColors().Length);
                Color newColor = BoardFunctions.GetAvailableColors() [newColorIndex];

                GameObject defaultImageChild = new GameObject("defaultImageChild");
                defaultImageChild.AddComponent<Image>();
                defaultImageChild.GetComponent<Image>().sprite = AssetLoader.GetDefaultElementSprite();
                defaultImageChild.GetComponent<Image>().preserveAspect = true;

                defaultImageChild.transform.parent = gamePanel.GetChild(i);
                defaultImageChild.transform.localScale = Vector3.one;
                defaultImageChild.transform.position = Vector3.zero;
                defaultImageChild.transform.SetAsFirstSibling();

                positions[collumIndex, rowIndex] = new BoardElement(gamePanel.GetChild(i).gameObject, i, newColor); //gamePanel.GetChild(i).gameObject.AddComponent<BoardElement>();

                playingAnimations.Add(new AnimationMessage(i, AssetLoader.GetDefaultElementSprite(), newColor));
                //Debug.Log(positions[collumIndex, rowIndex].name + " is at position: c" + collumIndex + ", r" + rowIndex);
                collumIndex++;
                // fix collum index
                if (collumIndex == totalCollums) {
                    rowIndex++;
                    collumIndex = 0;
                }
            }

        }
        SendAnimationMessagesToClient(ref playingAnimations);
    }

    // Start is called before the first frame update
    void Start() {
        // Assign color values to elemets

        //int currentScore = 0;
        bool hasPotentialInputs = false;
        for (int row = 0; row < positions.GetLength(1); row++) {
            for (int collum = 0; collum < positions.GetLength(0); collum++) {
                //positions[collum, row].SetValue(BoardFunctions.GetColorInScore(collum, row, ref positions, matchedElemPositions, totalCollums, totalRows, maxScoreAllowed, ref currentScore, (currentScore >= maxScoreAllowed - 2) ? true : false));
                if (BoardFunctions.isPotentialInput(collum, row, positions, matchedElemPositions, totalCollums, totalRows) > 0 && !hasPotentialInputs) {
                    hasPotentialInputs = true;
                }

            }
        }
        shouldCheckBoard = true;
    }

    // Update is called once per frame
    void Update() {
        // Do nothing.
        if (areAnimationsPlaying) {
            return;
        }
        // Step1: check if input create matches and if true move to step2
        else if (shouldCheckIfInputCreatesMatches) {
            AlexDebugger.GetInstance().AddMessage("Entering Step1: -check if input between-" + firstElement.GetAttachedGameObject().transform.name + " and " + secondElement.GetAttachedGameObject().transform.name + " create matches", AlexDebugger.tags.Step1);
            //Debug.Log("Checking for matches based on input");
            // About first input
            if (HandleInputForElement(firstElement, secondElement) || HandleInputForElement(secondElement, firstElement)) {

                shouldPlayEffects = true;
            }
            // About second input

            if (!shouldPlayEffects) {
                AlexDebugger.GetInstance().AddMessage("No matches found, when finish, go back to step0: -waiting for input-", AlexDebugger.tags.Step1);
            }

            firstElement = null;
            secondElement = null;
            shouldCheckIfInputCreatesMatches = false;
            return;
        }
        // Step2: -Play effects for matched elements- and move to step3
        else if (shouldPlayEffects) {
            AlexDebugger.GetInstance().AddMessage("Entering Step2: -play effects for matched elements-", AlexDebugger.tags.Step2);
            BoardElement lastElementProcessed = null;
            bool areThereChangesOnBoard = false;
            for (int row = 0; row < positions.GetLength(1); row++) {
                for (int collum = 0; collum < positions.GetLength(0); collum++) {
                    if (matchedElemPositions[collum, row] == true) {
                        if (BoardFunctions.DestroyBoardElement(collum, row, ref positions, ref matchedElemPositions, ref playingAnimations, lastElementProcessed)) {
                            areThereChangesOnBoard = true;
                        }
                        BoardFunctions.PlayMatchEffect(collum, row, positions, ref playingAnimations, swappingSpeed);
                    }
                    lastElementProcessed = positions[collum, row];
                    BoardFunctions.ToggleHighlightCell(collum, row, positions, false, highlightColor);
                }
            }

            if (!areThereChangesOnBoard) {
                shouldPlayEffects = false;
                AlexDebugger.GetInstance().AddMessage("Sending total animations to client: " + playingAnimations.Count, AlexDebugger.tags.Step2);
                SendAnimationMessagesToClient(ref playingAnimations);
                AlexDebugger.GetInstance().AddMessage("Step2 finished: -play effects for matched elements-, go to Step3: -reorient elements- " + playingAnimations.Count, AlexDebugger.tags.Step2);
                areElemetsReorienting = true;
            }
            else {
                AlexDebugger.GetInstance().AddMessage("Step2 finished: -play effects for matched elements-, new elements has been destroyed, repeating step ", AlexDebugger.tags.Step2);
            }
            return;
        }
        // Step3: repeat until non-destroyed elements have droped to lower position and then move to step4
        else if (areElemetsReorienting) {
            AlexDebugger.GetInstance().AddMessage("Entering Step3: -reorienting board-", AlexDebugger.tags.Step3);
            bool hasChanged = false;
            for (int row = totalRows - 1; row > -1; row--) {
                for (int collum = 0; collum < totalCollums; collum++) {
                    if (matchedElemPositions[collum, row] == true) {
                        AlexDebugger.GetInstance().AddMessage("Moving element: " + positions[collum, row].GetAttachedGameObject().transform.name + " upwards", AlexDebugger.tags.Step3);
                        BoardFunctions.MoveMatchedElementUpwards(collum, row, ref positions, ref matchedElemPositions, ref positionsDestroyed, ref playingAnimations, swappingSpeed);
                        AlexDebugger.GetInstance().AddMessage("Sending total animations to client: " + playingAnimations.Count, AlexDebugger.tags.Step3);
                        SendAnimationMessagesToClient(ref playingAnimations);
                        hasChanged = true;
                    }
                }
                // placed here so the entire row's animation play simutaniously
                if (hasChanged) {
                    AlexDebugger.GetInstance().AddMessage("Re-orienting has not finished, wait a frame and repeat Step3: -reorienting board-", AlexDebugger.tags.Step3);
                    return;
                }
            }
            if (hasChanged) {
                AlexDebugger.GetInstance().AddMessage("Re-orienting has not finished, wait a frame and repeat Step3: -reorienting board-", AlexDebugger.tags.Step3);
                return;
            }

            SendAnimationMessagesToClient(ref playingAnimations);
            AlexDebugger.GetInstance().AddMessage("Step3: -reorienting board- has finished, moving to Step4: -replace destroyed elements", AlexDebugger.tags.Step3);
            areElemetsReorienting = false;
            shouldGenerateElem = true;

            return;
        }
        // Step4: -Replace destroyed elements-, by assigning new color,  move to step5
        else if (shouldGenerateElem) {
            AlexDebugger.GetInstance().AddMessage("Entering Step4: -replace destroyed elements-", AlexDebugger.tags.Step4);
            bool[, ] searchedElements = new bool[positionsDestroyed.GetLength(0), positionsDestroyed.GetLength(1)];
            for (int row = 0; row < positions.GetLength(1); row++) {
                for (int collum = 0; collum < positions.GetLength(0); collum++) {
                    searchedElements[collum, row] = false;
                }
            }
            for (int row = 0; row < positions.GetLength(1); row++) {
                for (int collum = 0; collum < positions.GetLength(0); collum++) {
                    if (positionsDestroyed[collum, row] == true) {
                        if (BoardFunctions.GetIfMatchCreatesBell(collum, row, positions, ref searchedElements)) {
                            positions[collum, row] = BoardFunctions.CreateNewElement(positions[collum, row], typeof(BellBoardElement));
                        }
                        else if (BoardFunctions.GetIfMatchCreatesBomb(collum, row, positions, ref searchedElements)) {
                            positions[collum, row] = BoardFunctions.CreateNewElement(positions[collum, row], typeof(BombBoardElement));
                        }
                        else if (BoardFunctions.GetIfMatchCreatesCross(collum, row, positions, ref searchedElements)) {
                            positions[collum, row] = BoardFunctions.CreateNewElement(positions[collum, row], typeof(CrossBoardElement));
                        }
                        else if (BoardFunctions.GetChances(FixedElementData.chanchesForCashElement)) {
                            positions[collum, row] = BoardFunctions.CreateNewElement(positions[collum, row], typeof(CashBoardElement));
                        }
                        else {
                            positions[collum, row] = BoardFunctions.CreateNewElement(positions[collum, row], typeof(BoardElement));
                        }
                        BoardFunctions.ReplaceElement(collum, row, ref positions, ref matchedElemPositions, holders, ref playingAnimations, swappingSpeed);
                        positionsDestroyed[collum, row] = false;

                    }
                }
            }

            AlexDebugger.GetInstance().AddMessage("Sending total animations to client: " + playingAnimations.Count, AlexDebugger.tags.Step4);
            SendAnimationMessagesToClient(ref playingAnimations);
            // AlexDebugger.GetInstance().AddMessage("Element: " + positions[bestInputCollum, bestInputRow].GetAttachedGameObject().name + " has the best score possible: " + maxOutput, AlexDebugger.tags.Step4);
            AlexDebugger.GetInstance().AddMessage("Step4 -Replace destroyed elements- has finished, moving to Step5 -Aftermatch-", AlexDebugger.tags.Step4);
            shouldGenerateElem = false;
            shouldCheckBoard = true;
            return;
        }
        // Step5: check for new matches
        else if (shouldCheckBoard) {
            AlexDebugger.GetInstance().AddMessage("Entering Step5: -Aftermatch-", AlexDebugger.tags.Step5);
            //Debug.Log("### AfterMatch ### Checking board for combos");
            int newTotalMatches = BoardFunctions.CheckBoardForMatches(positions, ref matchedElemPositions, totalCollums, totalRows);
            // Go back to step2
            if (newTotalMatches > 0) {
                //Debug.Log("### AfterMatch ### Total new matches found: " + newTotalMatches);
                AlexDebugger.GetInstance().AddMessage("Total new matches found on board: " + newTotalMatches, AlexDebugger.tags.Step5);
                AlexDebugger.GetInstance().AddMessage("Step5 -Aftermatch- has finished, with state -new matches found-,  moving to Step2 -Play effects for matched elements-", AlexDebugger.tags.Step5);
                shouldPlayEffects = true;
            }
            // Check for possible inputs
            else {
                AlexDebugger.GetInstance().AddMessage("No new matches found, flagging potential input", AlexDebugger.tags.Step5);
                bool hasPossibleInput = false;
                for (int row = 0; row < positions.GetLength(1); row++) {
                    for (int collum = 0; collum < positions.GetLength(0); collum++) {
                        if (BoardFunctions.isPotentialInput(collum, row, positions, matchedElemPositions, totalCollums, totalRows) > 0) {
                            AlexDebugger.GetInstance().AddMessage(positions[collum, row].GetAttachedGameObject() + " is potential input", AlexDebugger.tags.Step5);
                            BoardFunctions.ToggleHighlightCell(collum, row, positions, true, highlightColor);
                            hasPossibleInput = true;
                        }
                        else {
                            BoardFunctions.ToggleHighlightCell(collum, row, positions, false, highlightColor);
                        }
                    }
                }
                //Debug.Log("### AfterMatch ### No new matches found");

                if (!hasPossibleInput) {
                    //Debug.Log("### AfterMatch ### No possible inputs found, re-generating board");
                    AlexDebugger.GetInstance().AddMessage("No potential input found, all elements are flagged as destroyed", AlexDebugger.tags.Step5);
                    for (int row = 0; row < positions.GetLength(1); row++) {
                        for (int collum = 0; collum < positions.GetLength(0); collum++) {
                            positionsDestroyed[collum, row] = true;
                        }
                    }
                    AlexDebugger.GetInstance().AddMessage("Step5 -Aftermatch- has finished, with state -No available input-,  moving to Step4 -Replace destroyed elements-", AlexDebugger.tags.Step5);
                    shouldGenerateElem = true;
                }
                else {
                    AlexDebugger.GetInstance().AddMessage("Step5 -Aftermatch- has finished, with state -Has available input-,  moving to Step0 -Wait for input-", AlexDebugger.tags.Step5);

                }
            }
            shouldCheckBoard = false;
            SendAnimationMessagesToClient(ref playingAnimations);
        }
        // New input can be processed
        else {
            OnDragEvent.hasDragBegin = false;
            areCellsSwapping = false;
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
        firstElement = BoardFunctions.GetElementBasedOnParentIndex(positions, firstElementIndexAtParent);
        // find second element based on gamePanel's child index
        secondElement = BoardFunctions.GetElementBasedOnParentIndex(positions, secondElementIndexAtParent);

        // Check if elements are neighbours thus input correct
        // TO-DO: make it to check if it is a possible mathch
        if (BoardFunctions.GetIfNeighbours(firstElement, secondElement, positions)) {
            AlexDebugger.GetInstance().AddMessage("Correct input: " + firstElement.GetAttachedGameObject().transform.name + ", with " + secondElement.GetAttachedGameObject().transform.name, AlexDebugger.tags.Input);
            // Remove tokens
            MoneyManager.ChangeBalanceBy(-MoneyManager.GetSwapCost());
            // Swap the elements on the board
            BoardFunctions.SwapElements(firstElement, secondElement, ref positions, ref playingAnimations, rewire : false, swappingSpeed);
            // Allow Update() to check if matches are created
            shouldCheckIfInputCreatesMatches = true;
        }
        else {
            // Swap elements, on rewire mode
            BoardFunctions.SwapElements(firstElement, secondElement, ref positions, ref playingAnimations, rewire : true, swappingSpeed);
        }
        // Send animations to client and empty list
        SendAnimationMessagesToClient(ref playingAnimations);
        areCellsSwapping = true;

    }

    public bool HandleInputForElement(BoardElement element, BoardElement otherElement) {
        bool areThereMatches = false;
        if (element.GetElementClassType() == typeof(BombBoardElement)) {
            if (otherElement.GetElementClassType() == typeof(CrossBoardElement)) {
                ((BombBoardElement) element).SetExplosionStyleTo(BombBoardElement.BombExplosionStyle.CrossStyle);
            }
            else if (otherElement.GetElementClassType() == typeof(BombBoardElement)) {
                ((BombBoardElement) element).SetExplosionStyleTo(BombBoardElement.BombExplosionStyle.DoubleBombStyle);
            }
            element.OnElementDestruction(positions, ref matchedElemPositions);
            AlexDebugger.GetInstance().AddMessage("first element was a bomb, go to step2: -play effects for matched elements-", AlexDebugger.tags.Step1);
            areThereMatches = true;
        }
        else if (element.GetElementClassType() == typeof(BellBoardElement)) {
            element.OnElementDestruction(ref positions, ref matchedElemPositions, ref playingAnimations, otherElement);
            areThereMatches = true;
        }
        else {
            KeyValuePair<int, int> firstPosition = BoardFunctions.GetPositionOfElement(element, positions);
            int numberOfMatchesForFirst = BoardFunctions.CheckForMatches(firstPosition.Key, firstPosition.Value, positions, ref matchedElemPositions, totalCollums, totalRows, true);
            AlexDebugger.GetInstance().AddMessage("Number of matches found for input element: " + element.GetAttachedGameObject().transform.name + " are " + numberOfMatchesForFirst, AlexDebugger.tags.Step1);
            // if there are matches
            if (numberOfMatchesForFirst > 0) {
                AlexDebugger.GetInstance().AddMessage("Matches found for first eleme " + numberOfMatchesForFirst + ", go to step2: -play effects for matched elements-", AlexDebugger.tags.Step1);
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
    }

    /// <summary>Is boardManager available</summary>
    public bool IsAvailable() {
        if (areCellsSwapping || areAnimationsPlaying) {
            return false;
        }
        else {
            return true;
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