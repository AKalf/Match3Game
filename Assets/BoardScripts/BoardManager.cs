using System;
using System.Collections.Generic;
using System.Numerics;

public class BoardManager : UnityEngine.MonoBehaviour {

    Random random = new Random();

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

    /// <summary> The elements of the board. 0 dimension is collums, first dimension is rows</summary>
    public BoardElement[, ] elementsPositions = null;
    /// <summary> Flagged elements that create matches</summary>
    public bool[, ] matchedElemPositions = null;
    public bool[, ] possibleInput = null;
    public bool[, ] changedPotitions = null;

#region  Utility and holders
    /// <summary> Value holder for step1 to step2</summary>
    BoardElement firstElement = null;
    /// <summary> Value holder for step1 to step2</summary>
    BoardElement secondElement = null;
#endregion

    // TO-DO: Create a proper singleton

    /// <summary> The instance of the manager</summary>
    public static BoardManager inst;

    /// <summary>Animations currently playing / qued to play. At server-client relationship this will be send to client to be played</summary>
    public List<Messages> messagesToClient = new List<Messages>();

    /// <summary> should be set false by the client</summary>
    public static bool areAnimationsPlaying = false;

    GameClient attachedClient = null;

    public int currentMessageID = -1;
    void Awake() {
        inst = this;

        elementsPositions = new BoardElement[ConstantValues.totalCollums, ConstantValues.totalRows];
        matchedElemPositions = new bool[ConstantValues.totalCollums, ConstantValues.totalRows];
        possibleInput = new bool[ConstantValues.totalCollums, ConstantValues.totalRows];
        changedPotitions = new bool[ConstantValues.totalCollums, ConstantValues.totalRows];
        // Debug
        //DebugCheckPanels();

        int row = 0;
        int collum = 0;

        // Assign elements to the board

        // foreach gameobject child of gameobject "GamePanel"
        for (int i = 0; i < ConstantValues.totalCollums * ConstantValues.totalRows; i++) {
            // if in set bounds
            if (collum < ConstantValues.totalCollums && row < ConstantValues.totalRows) {

                // Get a random color
                int randomNum = random.Next(0, ConstantValues.GetAvailableColors().Length);

                Vector4 newColor = ConstantValues.GetAvailableColors() [randomNum];

                // Create a new BoardElement instance an assign it to positions board
                elementsPositions[collum, row] = new BoardElement(new int[] { 0, 1, i }, newColor);
                // new animation to show the default sprite on gameobject
                currentMessageID += 1;
                messagesToClient.Add(new Messages.AnimationMessage(currentMessageID, -1, Messages.AnimationMessage.AnimationMessageTypes.ChangeSprite, elementsPositions[collum, row].GetImageTransformIndex(), (int) ConstantValues.AvailableSprites.defaultElement, newColor));
                changedPotitions[collum, row] = true;
                collum++;
                // fix collum index
                if (collum == ConstantValues.totalCollums) {
                    row++;
                    collum = 0;
                }
            }
            else {
                UnityEngine.Debug.LogError("There are more transforms on the board than collums * rows");
            }

        }
        AddWaitMessage();
        SendMessagesToClient();
    }

    // Start is called before the first frame update
    void Start() {
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
                BoardFunctions.SwapElements(firstElement, secondElement, this, rewire : false, ConstantValues.swappingSpeed / 2);
                //Debug.Log("Checking for matches based on input");
                // check if input create matches
                if (HandleInputForElement(firstElement, secondElement)) {
                    if (firstElement.GetElementClassType() != typeof(BellBoardElement)) {
                        HandleInputForElement(secondElement, firstElement);
                    }
                    //AlexDebugger.GetInstance().AddMessage("Step1 finished with matches, going to Step2: -play effects for matched elements-, input1:" + firstElement.GetAttachedGameObject().transform.name + ", input2: " + secondElement.GetAttachedGameObject().transform.name, AlexDebugger.tags.Step1);
                    // Remove tokens
                    MoneyManager.ChangeBalanceBy(-MoneyManager.GetSwapCost());
                    // Swap the elements on the board
                    // Allow Update() to check if matches are created
                    AddWaitMessage();
                    currentMessageID += 1;
                    Server.GetServerInstance().SendMessageToClient(new Messages.ServerStatusMessage(currentMessageID, -1, false));
                    currentStep = GameStep.PlayingEffects;
                }
                else if (HandleInputForElement(secondElement, firstElement)) {
                    //AlexDebugger.GetInstance().AddMessage("Step1 finished with matches, going to Step2: -play effects for matched elements-, input1:" + firstElement.GetAttachedGameObject().transform.name + ", input2: " + secondElement.GetAttachedGameObject().transform.name, AlexDebugger.tags.Step1);
                    // Remove tokens
                    MoneyManager.ChangeBalanceBy(-MoneyManager.GetSwapCost());
                    // Swap the elements on the board
                    AddWaitMessage();
                    Server.GetServerInstance().SendMessageToClient(new Messages.ServerStatusMessage(currentMessageID, -1, false));

                    currentStep = GameStep.PlayingEffects;
                }
                else {
                    BoardFunctions.SwapElements(firstElement, secondElement, this, rewire : false);
                    AddWaitMessage();
                    AlexDebugger.GetInstance().AddMessage("No matches found, when finish, go back to step0: -waiting for input-", AlexDebugger.tags.Step1);
                    SendMessagesToClient();
                    currentMessageID += 1;
                    Server.GetServerInstance().SendMessageToClient(new Messages.ServerStatusMessage(currentMessageID, -1, true));
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
                    AddWaitMessage();
                    currentStep = GameStep.OrientingElements;
                }
                break;

                // Step3: repeat until non-destroyed elements have droped to lower position and then move to step4
            case GameStep.OrientingElements:
                AlexDebugger.GetInstance().AddMessage("Entering Step3: -reorienting board-", AlexDebugger.tags.Step3);
                ReorientElements();
                AddWaitMessage();
                //AlexDebugger.GetInstance().AddMessage("Step3: -reorienting board- has finished, moving to Step4: -replace destroyed elements", AlexDebugger.tags.Step3);
                currentStep = GameStep.GeneratingElements;
                break;
                // Step4: -Replace destroyed elements-, by assigning new color,  move to step5
            case GameStep.GeneratingElements:
                //AlexDebugger.GetInstance().AddMessage("Entering Step4: -replace destroyed elements-", AlexDebugger.tags.Step4);
                GenerateNewElemetns();
                //AlexDebugger.GetInstance().AddMessage("Sending total animations to client: " + playingAnimations.Count, AlexDebugger.tags.Step4);
                AddWaitMessage();
                // AlexDebugger.GetInstance().AddMessage("Element: " + positions[bestInputCollum, bestInputRow].GetAttachedGameObject().name + " has the best score possible: " + maxOutput, AlexDebugger.tags.Step4);
                //AlexDebugger.GetInstance().AddMessage("Step4 -Replace destroyed elements- has finished, moving to Step5 -Aftermatch-", AlexDebugger.tags.Step4);
                currentStep = GameStep.CheckingBoardForMatches;
                break;
                // Step5: check for new matches
            case GameStep.CheckingBoardForMatches:
                AlexDebugger.GetInstance().AddMessage("Entering Step5: -Aftermatch-", AlexDebugger.tags.Step5);
                //Debug.Log("### AfterMatch ### Checking board for combos");
                int newTotalMatches = CheckBoardForMatches();
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
                AddWaitMessage();
                break;
            case GameStep.MarkingPossibleInputs:
                //Debug.Log("### AfterMatch ### No new matches found");
                if (!CheckForPossibleInputs()) {
                    currentMessageID += 1;
                    messagesToClient.Add(new Messages.AnimationMessage.PopBoxMessage(currentMessageID, currentMessageID - 1, "No possible inputs found. Regenerating board.", -1));
                    //Debug.Log("### AfterMatch ### No possible inputs found, re-generating board");
                    //AlexDebugger.GetInstance().AddMessage("No potential input found, all elements are flagged as destroyed", AlexDebugger.tags.Step5);
                    MarkAllElementsAsDestroyed();
                    //AlexDebugger.GetInstance().AddMessage("Step5 -Aftermatch- has finished, with state -No available input-,  moving to Step4 -Replace destroyed elements-", AlexDebugger.tags.Step5);
                    currentStep = GameStep.GeneratingElements;
                }
                else {
                    AlexDebugger.GetInstance().AddMessage("Step5 -Aftermatch- has finished, with state -Has available input-,  moving to Step0 -Wait for input-", AlexDebugger.tags.Step5);
                    currentMessageID += 1;
                    Server.GetServerInstance().SendMessageToClient(new Messages.ServerStatusMessage(currentMessageID, -1, true));
                    currentStep = GameStep.WaitingInput;
                }
                AddWaitMessage();
                SendMessagesToClient();
                break;
        }
    }

    public static BoardManager GetInstance() {
        return inst;
    }

    /// <summary>Receives input from client and handles it</summary>
    public void TakeInput(int firstElementIndexAtParent, int secondElementIndexAtParent) {
        // find first element based on gamePanel's child index
        firstElement = BoardFunctions.GetElementBasedOnParentIndex(elementsPositions, firstElementIndexAtParent);
        // find second element based on gamePanel's child index
        secondElement = BoardFunctions.GetElementBasedOnParentIndex(elementsPositions, secondElementIndexAtParent);
        currentStep = GameStep.CheckingInput;

#region  Debug
        //  To use for cheats
        // if (BoardFunctions.GetIfNeighbours(firstElement, secondElement, elementsPositions)) {
        //     AlexDebugger.GetInstance().AddMessage("Correct input: " + BoardFunctions.GetTransformByIndex(firstElement.GetTransformIndex()) + ", with " + BoardFunctions.GetTransformByIndex(secondElement.GetTransformIndex()), AlexDebugger.tags.Input);
        //     // Remove tokens
        //     MoneyManager.ChangeBalanceBy(-MoneyManager.GetSwapCost());
        //     // Swap the elements on the board
        //     BoardFunctions.SwapElements(firstElement, secondElement, this, rewire : false, FixedElementData.swappingSpeed / 2);
        //     // Allow Update() to check if matches are created

        // }
        // else {
        //     // Swap elements, on rewire mode
        //     BoardFunctions.SwapElements(firstElement, secondElement, this, rewire : true);
        //     AddWaitMessage();
        //     SendMessagesToClient();
        //     currentMessageID += 1;
        //     Server.GetServerInstance().SendMessageToClient(new Messages.ServerStatusMessage(currentMessageID, -1, true));

        // }
#endregion
    }

    private bool PlayEffectsStep() {
        BoardElement lastElementProcessed = secondElement;
        bool areThereChangesOnBoard = false;
        for (int row = 0; row < elementsPositions.GetLength(1); row++) {
            for (int collum = 0; collum < elementsPositions.GetLength(0); collum++) {
                if (matchedElemPositions[collum, row] == true) {
                    BoardFunctions.PlayMatchEffectAnimations(collum, row, this);
                    if (BoardFunctions.DestroyBoardElement(collum, row, this, lastElementProcessed)) {
                        areThereChangesOnBoard = true;
                    }

                }
                lastElementProcessed = elementsPositions[collum, row];
                BoardFunctions.ToggleHighlightCell(collum, row, this, false);
            }
        }

        if (!areThereChangesOnBoard) {
            AlexDebugger.GetInstance().AddMessage("Step2 finished: -play effects for matched elements-, go to Step3: -reorient elements- " + messagesToClient.Count, AlexDebugger.tags.Step2);
            return true;
        }
        else {
            AlexDebugger.GetInstance().AddMessage("Step2 finished: -play effects for matched elements-, new elements has been destroyed, repeating step ", AlexDebugger.tags.Step2);
            return false;
        }
    }

    private void ReorientElements() {
        bool hasSFXplayed = false;
        for (int collum = 0; collum < ConstantValues.totalCollums; collum++) {
            //AlexDebugger.GetInstance().AddMessage("Moving element: " + elementsPositions[collum, row].GetAttachedGameObject().transform.name + " upwards", AlexDebugger.tags.Step3);
            if (BoardFunctions.MoveMatchedElementUpwards(collum, ConstantValues.totalRows - 1, this) && !hasSFXplayed) {
                float randomNum = random.Next(5, 20) / 10;
                currentMessageID += 1;
                messagesToClient.Add(new Messages.AudioMessage(AssetLoader.GetCellDropSFX(), currentMessageID, currentMessageID - 1, 0.0f, 1));
                hasSFXplayed = true;
            }

        }
    }

    private void GenerateNewElemetns() {

        bool[, ] searchedElements = new bool[matchedElemPositions.GetLength(0), matchedElemPositions.GetLength(1)];
        for (int row = 0; row < elementsPositions.GetLength(1); row++) {
            for (int collum = 0; collum < elementsPositions.GetLength(0); collum++) {
                if (matchedElemPositions[collum, row] == true) {
                    BoardFunctions.ReplaceElementAnimations(collum, row, this, ref searchedElements);
                    matchedElemPositions[collum, row] = false;
                    changedPotitions[collum, row] = true;

                }
            }
        }
        float randomNum = random.Next(5, 20) / 10;
        currentMessageID += 1;
        messagesToClient.Add(new Messages.AudioMessage(AssetLoader.GetCellDropSFX(), currentMessageID, currentMessageID - 1, 0.0f, 1));

    }

    /// <summary>Checks each element on the board for matches and flags them</summary>
    public int CheckBoardForMatches() {
        AlexDebugger.GetInstance().AddMessage("Checking board for new matches...", AlexDebugger.tags.Aftermatch);
        int totalMatches = 0;
        // Search for matches and flag them
        for (int row = 0; row < elementsPositions.GetLength(1); row++) {
            for (int collum = 0; collum < elementsPositions.GetLength(0); collum++) {
                // case of cash element has reached bottom
                if (row == elementsPositions.GetLength(1) - 1 && elementsPositions[collum, row].GetElementClassType() == typeof(CashBoardElement)) {
                    matchedElemPositions[collum, row] = true;
                    AlexDebugger.GetInstance().AddMessage("A cash element has reached bottom at position: " + collum + ", " + row, AlexDebugger.tags.Aftermatch);
                    totalMatches++;
                }
                else if (matchedElemPositions[collum, row] == true || !changedPotitions[collum, row]) {
                    continue;
                }
                else {
                    totalMatches += BoardFunctions.CheckElementForMatches(collum, row, elementsPositions, ref matchedElemPositions, ConstantValues.totalCollums, ConstantValues.totalRows, true);
                }
            }

        }
        return totalMatches;

    }

    private bool CheckForPossibleInputs() {
        bool hasPossibleInput = false;
        for (int row = 0; row < elementsPositions.GetLength(1); row++) {
            for (int collum = 0; collum < elementsPositions.GetLength(0); collum++) {
                changedPotitions[collum, row] = false;
                if (elementsPositions[collum, row].GetElementClassType() == typeof(CashBoardElement)) {
                    continue;
                }
                else if (elementsPositions[collum, row].GetElementClassType() == typeof(BombBoardElement) || elementsPositions[collum, row].GetElementClassType() == typeof(BellBoardElement)) {
                    hasPossibleInput = true;
                }
                else if (BoardFunctions.IsPotentialInput(collum, row, elementsPositions, matchedElemPositions, ConstantValues.totalCollums, ConstantValues.totalRows) > 0) {
                    //AlexDebugger.GetInstance().AddMessage(BoardFunctions.GetTransformByIndex(elementsPositions[collum, row].GetTransformIndex()) + " is potential input", AlexDebugger.tags.Step5);
                    BoardFunctions.ToggleHighlightCell(collum, row, this, true);
                    hasPossibleInput = true;
                }
                else {
                    BoardFunctions.ToggleHighlightCell(collum, row, this, false);
                }
            }
        }
        return hasPossibleInput;
    }

    private void MarkAllElementsAsDestroyed() {
        for (int row = 0; row < elementsPositions.GetLength(1); row++) {
            for (int collum = 0; collum < elementsPositions.GetLength(0); collum++) {
                matchedElemPositions[collum, row] = true;
            }
        }
    }

    public bool HandleInputForElement(BoardElement element, BoardElement otherElement) {
        bool areThereMatches = false;
        if (element.GetElementClassType() == typeof(CashBoardElement) || otherElement.GetElementClassType() == typeof(CashBoardElement)) {
            return false;
        }
        else if (element.GetElementClassType() == typeof(BombBoardElement)) {
            if (otherElement.GetElementClassType() == typeof(CrossBoardElement)) {
                //AlexDebugger.GetInstance().AddMessage(BoardFunctions.GetTransformByIndex(element.GetTransformIndex()) + " bomb style set to cross", AlexDebugger.tags.Step1);
                ((BombBoardElement) element).SetExplosionStyleTo(BombBoardElement.BombExplosionStyle.CrossStyle);
            }
            else if (otherElement.GetElementClassType() == typeof(BombBoardElement)) {
                //AlexDebugger.GetInstance().AddMessage(BoardFunctions.GetTransformByIndex(element.GetTransformIndex()) + " bomb style set to double bomb", AlexDebugger.tags.Step1);
                ((BombBoardElement) element).SetExplosionStyleTo(BombBoardElement.BombExplosionStyle.DoubleBombStyle);
            }
            element.OnElementDestruction(this);
            AlexDebugger.GetInstance().AddMessage("first element was a bomb, go to step2: -play effects for matched elements-", AlexDebugger.tags.Step1);
            areThereMatches = true;
        }
        else if (element.GetElementClassType() == typeof(BellBoardElement)) {
            AlexDebugger.GetInstance().AddMessage("first element was a bell, go to step2: -play effects for matched elements-", AlexDebugger.tags.Step1);
            element.OnElementDestruction(this, otherElement);
            areThereMatches = true;
        }
        else {
            KeyValuePair<int, int> firstPosition = BoardFunctions.GetBoardPositionOfElement(element, elementsPositions);
            int numberOfMatchesForFirst = BoardFunctions.CheckElementForMatches(firstPosition.Key, firstPosition.Value, elementsPositions, ref matchedElemPositions, ConstantValues.totalCollums, ConstantValues.totalRows, true);
            // if there are matches
            if (numberOfMatchesForFirst > 0) {
                AlexDebugger.GetInstance().AddMessage("Matches found for first element " + numberOfMatchesForFirst + ", go to step2: -play effects for matched elements-", AlexDebugger.tags.Step1);
                // allow Update() to play effects
                areThereMatches = true;
            }
        }
        return areThereMatches;
    }

    public void AddWaitMessage() {
        currentMessageID += 1;
        messagesToClient.Add(new Messages(currentMessageID, -1, Messages.MessageTypes.Wait));
        currentMessageID = -1;
    }

    public void SendMessagesToClient() {
        Server.GetServerInstance().SendMessagesToClient(messagesToClient);
        currentMessageID = -1;
        messagesToClient.Clear();
    }

    public GameClient GetClientAttached() {
        return attachedClient;
    }
    public void SetClient(GameClient clientToAttach) {
        if (attachedClient == null) {
            attachedClient = clientToAttach;
        }
        else {
            //Debug.LogWarning("Trying to attach client but client has already been attached");
        }
    }

    /// Debug
    //     private void DebugElementsMatches(List<BoardElement> elements) {
    //         if (elements.Count > 0) {
    //             string d = "";
    //             foreach (BoardElement e in elements) {
    //                 d += e.GetAttachedGameObject().name + ", ";
    //             }
    //             AlexDebugger.GetInstance().AddMessage(d, AlexDebugger.tags.Matches);
    //             Debug.Log(d + ", Matches");
    //         }
    //     }
    //     private void DebugCheckPanels() {
    // #if UNITY_EDITOR
    // if (detailsPanel.gameObject.name != "DetailsPanel") {
    //     Debug.LogError(boardPanel.gameObject.name + ".GetChild(0).name != DetailsPanel");
    // }
    // if (gamePanel.gameObject.name != "GamePanel") {
    //     Debug.LogError(boardPanel.gameObject.name + ".GetChild(1).name != GamePanel");
    // }
    // else {
    //     for (int i = 0; i < gamePanel.childCount; i++) {
    //         if (gamePanel.GetChild(i).GetComponent<UnityEngine.UI.Image>() == null) {
    //             Debug.LogError(gamePanel.GetChild(i).name + " has no Image component");
    //             gamePanel.GetChild(i).gameObject.AddComponent<UnityEngine.UI.Image>();
    //         }

    //     }
    // }

    // #endif
    //     }

}