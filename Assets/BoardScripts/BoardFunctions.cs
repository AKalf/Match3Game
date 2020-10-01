using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BoardFunctions {

    /// <summary> Possible values of cells</summary>
    public static Color[] availColors = { Color.red, Color.blue, Color.green, Color.cyan, Color.magenta };

    /// <summary>Returns the position of the element on the board</summary>
    public static KeyValuePair<int, int> GetPositionOfElement(BoardElement element, BoardElement[, ] positions) {
        for (int row = 0; row < positions.GetLength(1); row++) {
            for (int collumn = 0; collumn < positions.GetLength(0); collumn++) {
                if (positions[collumn, row] == element) {
                    return new KeyValuePair<int, int>(collumn, row);
                }
            }
        }
        Debug.LogError("Could not find element: " + element.GetAttachedGameObject().name + " on board!");
        return new KeyValuePair<int, int>();
    }

    /// <summary>Swaps to elements positions on the board </summary>
    public static void SwapBoardElementNeighbours(BoardElement oldElement, BoardElement newElement, ref BoardElement[, ] positions) {

        KeyValuePair<int, int> oldElementPosition = BoardFunctions.GetPositionOfElement(oldElement, positions);
        KeyValuePair<int, int> newElementPosition = BoardFunctions.GetPositionOfElement(newElement, positions);

        positions[oldElementPosition.Key, oldElementPosition.Value] = newElement;
        positions[newElementPosition.Key, newElementPosition.Value] = oldElement;

#if UNITY_EDITOR
        //Debug.Log(newElementPosition.Key + ", " + newElementPosition.Value + " set to position " + oldElementPosition.Key + ", " + oldElementPosition.Value);
        //Debug.Log(oldElementPosition.Key + ", " + oldElementPosition.Value + " set to position " + newElementPosition.Key + ", " + newElementPosition.Value);
        if (positions[oldElementPosition.Key, oldElementPosition.Value] == positions[newElementPosition.Key, newElementPosition.Value]) {
            Debug.LogError("Swap was not correct");
        }
#endif
    }

    /// <summary>Sets swapping animations and changes positions on the board if it is not gonna rewire </summary>
    /// <param name="firstElement"></param>
    /// <param name="secondElement"></param>
    /// <param name="positions">the board of elemetns</param>
    /// <param name="playingAnimations">list to add new animations</param>
    /// <param name="rewire">use if input is incorrect</param>
    /// <param name="shouldCheckMatches">should check for new matches after swap?</param>
    /// <param name="swappingSpeed">the speed of animation</param>
    /// <returns>Returns if should check</returns>
    public static void SwapElements(BoardElement firstElement, BoardElement secondElement, ref BoardElement[, ] positions, ref List<AnimationMessage> playingAnimations, bool rewire = false, float swappingSpeed = 0.2f) {

        Vector3 targetPos = secondElement.GetAttachedGameObject().transform.position;
        playingAnimations.Add(new AnimationMessage(Animations.AnimationTypes.MoveTo, firstElement.GetChildIndex(), swappingSpeed, targetPos.x, targetPos.y, targetPos.z));

        targetPos = firstElement.GetAttachedGameObject().transform.position;
        playingAnimations.Add(new AnimationMessage(Animations.AnimationTypes.MoveTo, secondElement.GetChildIndex(), swappingSpeed, targetPos.x, targetPos.y, targetPos.z));

        if (!rewire) {
            BoardFunctions.SwapBoardElementNeighbours(firstElement, secondElement, ref positions);
        }
        else {
            targetPos = firstElement.GetAttachedGameObject().transform.position;
            playingAnimations.Add(new AnimationMessage(Animations.AnimationTypes.MoveTo, firstElement.GetChildIndex(), swappingSpeed, targetPos.x, targetPos.y, targetPos.z));
            targetPos = secondElement.GetAttachedGameObject().transform.position;
            playingAnimations.Add(new AnimationMessage(Animations.AnimationTypes.MoveTo, secondElement.GetChildIndex(), swappingSpeed, targetPos.x, targetPos.y, targetPos.z));
        }
        // if (playingAnimations.Count > 0) {
        //     BoardManager.GetInstance().SendAnimationMessagesToClient(ref playingAnimations);
        // }
    }

    /// <summary> Returns the number of elements matched towards top </summary>
    public static int CheckUpperNeighboursForMatches(int collum, int row, BoardElement[, ] positions) {
        int numberOfElem = 0;

        if (row - 1 > -1) {
            if (positions[collum, row - 1].GetElementValue() == Color.white) {
                return 0;
            }
            if (positions[collum, row].GetElementValue() == positions[collum, row - 1].GetElementValue()) {
                numberOfElem++;
                numberOfElem += BoardFunctions.CheckUpperNeighboursForMatches(collum, row - 1, positions);
            }
        }
        return numberOfElem;
    }
    /// <summary> Returns the number of elements matched towards bottom </summary>
    public static int CheckBottomNeighboursForMatches(int collum, int row, BoardElement[, ] positions, int rowsNumbers) {
        int numberOfElem = 0;

        if (row + 1 < rowsNumbers) {
            if (positions[collum, row + 1].GetElementValue() == Color.white) {
                return 0;
            }
            if (positions[collum, row].GetElementValue() == positions[collum, row + 1].GetElementValue()) {
                numberOfElem++;
                numberOfElem += BoardFunctions.CheckBottomNeighboursForMatches(collum, row + 1, positions, rowsNumbers);
            }
        }
        return numberOfElem;

    }

    /// <summary> Returns the number of elements matched towards left </summary>
    public static int CheckLeftNeighboursForMatches(int collum, int row, BoardElement[, ] positions) {
        int numberOfElem = 0;

        if (collum - 1 > -1) {
            if (positions[collum - 1, row].GetElementValue() == Color.white) {
                return 0;
            }
            if (positions[collum, row].GetElementValue() == positions[collum - 1, row].GetElementValue()) {
                numberOfElem++;
                numberOfElem += BoardFunctions.CheckLeftNeighboursForMatches(collum - 1, row, positions);

            }

        }
        return numberOfElem;
    }
    /// <summary> Returns the number of elements matched towards right </summary>
    public static int CheckRightNeighboursForMatches(int collum, int row, BoardElement[, ] positions, int collumsNumber) {
        int numberOfElem = 0;
        if (collum + 1 < collumsNumber) {
            if (positions[collum + 1, row].GetElementValue() == Color.white) {
                return 0;
            }
            if (positions[collum, row].GetElementValue() == positions[collum + 1, row].GetElementValue()) {
                numberOfElem++;
                numberOfElem += BoardFunctions.CheckRightNeighboursForMatches(collum + 1, row, positions, collumsNumber);

            }
        }
        return numberOfElem;
    }

    /// <summary> Flags positions that belong to a match </summary>
    /// <param name="shouldFlag"> if false, elements wont be marked as matched (usefull when want to check possible matches)</param>
    /// <returns>returns an array where:
    /// position 0: number of matches that create a cross
    /// position 1: number of matches create a bomb
    /// position 2: number of matches create a bell</returns>
    public static int CheckForMatches(int startCollum, int startRow, BoardElement[, ] positions, ref bool[, ] matchedElemPositions, int collumsNumber, int rowsNumber, bool shouldFlag = true) {

        int upperMatches = BoardFunctions.CheckUpperNeighboursForMatches(startCollum, startRow, positions);
        int bottomMatches = BoardFunctions.CheckBottomNeighboursForMatches(startCollum, startRow, positions, rowsNumber);
        int leftMatches = BoardFunctions.CheckLeftNeighboursForMatches(startCollum, startRow, positions);
        int rightMatches = BoardFunctions.CheckRightNeighboursForMatches(startCollum, startRow, positions, collumsNumber);

        if (shouldFlag) {
            // if more than 2 element matches, add vertical mathced elements
            if (upperMatches + bottomMatches >= 2) {
                matchedElemPositions[startCollum, startRow] = true;
                for (int row = startRow - upperMatches; row <= startRow + bottomMatches; row++) {
                    matchedElemPositions[startCollum, row] = true;
                    // AlexDebugger.GetInstance().AddMessage(positions[startCollum, row].GetAttachedGameObject().name + "is an " + ((row < startRow) ? " on top" : "on bottom") + " match of " + positions[startCollum, row].GetAttachedGameObject().name, AlexDebugger.tags.Matches);
                }
                if (upperMatches > 0) {
                    AlexDebugger.GetInstance().AddMessage("Upper matches for element: " + positions[startCollum, startRow].GetAttachedGameObject().name + " are: " + upperMatches, AlexDebugger.tags.Matches);
                }
                if (bottomMatches > 0) {
                    AlexDebugger.GetInstance().AddMessage("Bottom matches for element: " + positions[startCollum, startRow].GetAttachedGameObject().name + " are: " + bottomMatches, AlexDebugger.tags.Matches);
                }
            }
            // if more than 2 element matches, add horizontal mathced elements
            if (leftMatches + rightMatches >= 2) {
                matchedElemPositions[startCollum, startRow] = true;
                for (int collum = startCollum - leftMatches; collum <= startCollum + rightMatches; collum++) {
                    matchedElemPositions[collum, startRow] = true;
                }
                if (leftMatches > 0) {
                    AlexDebugger.GetInstance().AddMessage("Left matches for element: " + positions[startCollum, startRow].GetAttachedGameObject().name + " are: " + leftMatches, AlexDebugger.tags.Matches);
                }
                if (rightMatches > 0) {
                    AlexDebugger.GetInstance().AddMessage("Bottom matches for element: " + positions[startCollum, startRow].GetAttachedGameObject().name + " are: " + rightMatches, AlexDebugger.tags.Matches);
                }
            }
            if (upperMatches + bottomMatches >= 2 || leftMatches + rightMatches >= 2) {
                AlexDebugger.GetInstance().AddMessage("Total matches found for: " + positions[startCollum, startRow].GetAttachedGameObject().name + " are: " + (upperMatches + bottomMatches + leftMatches + rightMatches).ToString(), AlexDebugger.tags.Matches);
            }
        }
        if (upperMatches + bottomMatches >= 2 || leftMatches + rightMatches >= 2) {
            return upperMatches + bottomMatches + leftMatches + rightMatches;
        }
        else {
            return 0;
        }
    }

    /// <summary> Used recursivly to move non-destroyed elements dowards</summary>
    public static void MoveMatchedElementUpwards(
        int collumn, int row,
        ref BoardElement[, ] positions, ref bool[, ] matchedElemPositions, ref bool[, ] positionsDestroyed,
        ref List<AnimationMessage> playingAnimations, float swappingSpeed) {
        AlexDebugger.GetInstance().AddMessage("Moving upwards " + positions[collumn, row].GetAttachedGameObject().name + " from position: " + collumn + ", " + row, AlexDebugger.tags.UpwardMovement);
        // swap with next non matched element on the collum if not on top of collum
        if (row != 0) {
            for (int i = row - 1; i > -1; i--) {
                // if non-matched element found above, swap
                if (matchedElemPositions[collumn, i] == false) {
                    AlexDebugger.GetInstance().AddMessage("Swapping " + positions[collumn, row].GetAttachedGameObject().name + " with " + positions[collumn, i].GetAttachedGameObject().name + " at position " + collumn + ", " + i, AlexDebugger.tags.UpwardMovement);
                    BoardFunctions.SwapElements(positions[collumn, row], positions[collumn, i], ref positions, ref playingAnimations, false, swappingSpeed);

                    // make changes to the flag array
                    matchedElemPositions[collumn, row] = false;
                    matchedElemPositions[collumn, i] = true;
                    return;
                }
            }
        }

        // if element reached top or there are no non-matched elements above, mark this element as destroyed and non-matched
        positionsDestroyed[collumn, row] = true;
        matchedElemPositions[collumn, row] = false;

    }

    /// <summary>Re-creates destroyed elements, by moving them to holders position and then making a drop effect</summary>
    /// <returns>The best score that can be reached if swapped with neighbour</returns>
    public static void ReplaceElement(int collum, int row, ref BoardElement[, ] positions, ref bool[, ] matchedElemPositions, Transform[] holders, ref List<AnimationMessage> playingAnimations, float swappingSpeed) {

        CheckIfElementAtPositionIsNull(collum, row, positions);

        AlexDebugger.GetInstance().AddMessage("Replacing " + positions[collum, row], AlexDebugger.tags.Effects);
        Vector3 originalPos = positions[collum, row].GetAttachedGameObject().transform.position;

#region Score check
        // the minimum maxOutput user's input can reach
        // int maxOutput = -1;

        // int bestInputCollum = -1;
        // int bestInputRow = -1;

        // // Check maxOutput for elements already on the board
        // for (int row = 0; row < positions.GetLength(1); row++) {
        //     for (int collum = 0; collum < positions.GetLength(0); collum++) {
        //         if (positionsDestroyed[collum, row] == false) {
        //             // Get the max output of this element
        //             int maxOutputForElement = BoardFunctions.isPotentialInput(collum, row, positions, matchedElemPositions, totalCollums, totalRows);
        //             if (maxOutputForElement >= 1) {
        //                 AlexDebugger.GetInstance().AddMessage("Element: " + positions[collum, row] + " exists with a potential output of " + maxOutputForElement, AlexDebugger.tags.Step4);
        //             }
        //             if (maxOutputForElement > maxOutput) {
        //                 AlexDebugger.GetInstance().AddMessage("Element: " + positions[collum, row] + " exists with a potential output of " + maxOutputForElement + " is the new possible maxOut, previous: " + maxOutput, AlexDebugger.tags.Step4);
        //                 maxOutput = maxOutputForElement;
        //                 bestInputCollum = collum;
        //                 bestInputRow = row;
        //             }
        //         }
        //     }
        // }

        //   for (int row = positions.GetLength(1) - 1; row > -1; row--) {
        //      for (int collum = 0; collum < positions.GetLength(0); collum++) {
        //if (positionsDestroyed[collum, row] == true) {

        // int maxScorePossible = 0;
        // if (maxScorePossible > maxScoreAllowed) {
        //     Debug.LogError("CurrentMaxScorePossible: " + maxScorePossible + " exceeds maxScoreAllowed: " + maxScorePossible);
        // }
#endregion

        playingAnimations.Add(new AnimationMessage(positions[collum, row].GetChildIndex(), positions[collum, row].GetElementSprite(), positions[collum, row].GetElementValue()));
#region Score check
        // if (maxScorePossible >= 1) {
        //     AlexDebugger.GetInstance().AddMessage("Element: " + positions[collum, row] + " was created wihh a potential output of " + maxScorePossible, AlexDebugger.tags.Step4);
        // }
        // if (maxScorePossible > maxOutput) {
        //     AlexDebugger.GetInstance().AddMessage("Element: " + positions[collum, row] + " exists with a potential output of " + maxScorePossible + " is the new possible maxOut, previous: " + maxOutput, AlexDebugger.tags.Step4);
        //     maxOutput = maxScorePossible;
        //     bestInputCollum = collum;
        //     bestInputRow = row;
        // }
        //}
        // }
        // }
#endregion
        playingAnimations.Add(new AnimationMessage(Animations.AnimationTypes.MoveTo, positions[collum, row].GetChildIndex(), 2000000, holders[collum].position.x, holders[collum].position.y, holders[collum].position.z));
        playingAnimations.Add(new AnimationMessage(Animations.AnimationTypes.ScaleToOne, positions[collum, row].GetChildIndex(), 200));
        playingAnimations.Add(new AnimationMessage(Animations.AnimationTypes.MoveTo, positions[collum, row].GetChildIndex(), swappingSpeed, originalPos.x, originalPos.y, originalPos.z));

        return;

    }

    /// <summary>Play scale-to-zero effect on element's transform</summary>
    public static void PlayMatchEffect(int collum, int row, BoardElement[, ] positions, ref List<AnimationMessage> playingAnimations, float swappingSpeed) {
        AlexDebugger.GetInstance().AddMessage("####### Scale to zero effect for " + positions[collum, row].GetAttachedGameObject().name, AlexDebugger.tags.Effects);

        playingAnimations.Add(new AnimationMessage(Animations.AnimationTypes.ScaleToZero, positions[collum, row].GetChildIndex(), swappingSpeed));
    }

    /// <summary>Checks each element on the board for matches and flags them</summary>
    public static int CheckBoardForMatches(BoardElement[, ] positions, ref bool[, ] matchedElemPositions, int collumsNumber, int rowsNumber) {
        AlexDebugger.GetInstance().AddMessage("Checking board for new matches...", AlexDebugger.tags.Aftermatch);
        int totalMatches = 0;
        // Search for matches and flag them
        for (int row = 0; row < positions.GetLength(1); row++) {
            for (int collum = 0; collum < positions.GetLength(0); collum++) {
                totalMatches += CheckForMatches(collum, row, positions, ref matchedElemPositions, collumsNumber, rowsNumber, true);
                // case of cash element has reached bottom
                if (row == positions.GetLength(1) - 1 && positions[collum, row].GetElementClassType() == typeof(CashBoardElement)) {
                    matchedElemPositions[collum, row] = true;
                    AlexDebugger.GetInstance().AddMessage("A cash element has reached bottom at position: " + collum + ", " + row, AlexDebugger.tags.Aftermatch);
                    totalMatches++;
                }

            }

        }
        return totalMatches;

    }

    /// <summary>Check if with input it can create matches. Returns best score found </summary>
    public static int isPotentialInput(int collum, int row, BoardElement[, ] positions, bool[, ] matchedElemPositions, int totalCollums, int totalRows) {
        int possibleScore = 0;
        if (row - 1 > -1) {
            BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum, row - 1], ref positions);
            int matches = BoardFunctions.CheckForMatches(collum, row - 1, positions, ref matchedElemPositions, totalCollums, totalRows, false);
            if (matches > 0) {
                BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum, row - 1], ref positions);
                possibleScore = matches;
            }
            else {
                BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum, row - 1], ref positions);
            }
        }
        if (row + 1 < totalRows) {
            BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum, row + 1], ref positions);
            int matches = BoardFunctions.CheckForMatches(collum, row + 1, positions, ref matchedElemPositions, totalCollums, totalRows, false);
            if (matches > 0) {
                BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum, row + 1], ref positions);
                if (possibleScore < matches) {
                    possibleScore = matches;
                }

            }
            else {
                BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum, row + 1], ref positions);
            }
        }
        if (collum - 1 > -1) {
            BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum - 1, row], ref positions);
            int matches = BoardFunctions.CheckForMatches(collum - 1, row, positions, ref matchedElemPositions, totalCollums, totalRows, false);
            if (matches > 0) {
                BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum - 1, row], ref positions);
                if (possibleScore < matches) {
                    possibleScore = matches;
                }
            }
            else {
                BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum - 1, row], ref positions);
            }
        }
        if (collum + 1 < totalCollums) {
            BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum + 1, row], ref positions);
            int matches = BoardFunctions.CheckForMatches(collum + 1, row, positions, ref matchedElemPositions, totalCollums, totalRows, false);
            if (matches > 0) {
                BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum + 1, row], ref positions);
                if (possibleScore < matches) {
                    possibleScore = matches;
                }
            }
            else {
                BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum + 1, row], ref positions);
            }
        }
        return possibleScore;
    }

    /// <summary> Returns if two elements are neighbours</summary>
    public static bool GetIfNeighbours(BoardElement elem1, BoardElement elem2, BoardElement[, ] positions) {
        KeyValuePair<int, int> elem1Pos = BoardFunctions.GetPositionOfElement(elem1, positions);
        KeyValuePair<int, int> elem2Pos = BoardFunctions.GetPositionOfElement(elem2, positions);
        // Case same collumn, one row         above                                 or below
        if (elem1Pos.Key == elem2Pos.Key && (elem2Pos.Value == elem1Pos.Value - 1 || elem2Pos.Value == elem1Pos.Value + 1)) {
            return true;
        }
        // Case same row, one collumn                left                                 or right
        else if (elem1Pos.Value == elem2Pos.Value && (elem2Pos.Key == elem1Pos.Key - 1 || elem2Pos.Key == elem1Pos.Key + 1)) {
            return true;
        }
        else {
            return false;
        }
    }
    /// <summary> Returns if two elements are neighbours</summary>
    public static bool GetIfNeighbours(int elem1Collum, int elem1row, int elem2Collum, int elem2row, BoardElement[, ] positions) {
        // Case same collumn, one row         above                                 or below
        if (elem1Collum == elem2Collum && (elem2row == elem1row - 1 || elem2row == elem1row + 1)) {
            return true;
        }
        // Case same row, one collumn                left                                 or right
        else if (elem1row == elem2row && (elem2Collum == elem1Collum - 1 || elem2Collum == elem1Collum + 1)) {
            return true;
        }
        else {
            return false;
        }
    }
    /// <summary> Get a color that does not exceeds max score allowed </summary>
    public static Color GetColorInScore(int collum, int row, ref BoardElement[, ] positions, bool[, ] matchedElemPositions, int totalCollums, int totalRows, int maxScoreAllowed, ref int currentScorePossible, bool forceGetMin) {
        int bestPossibleScoreFound = 0;
        int bestScoreIndex = UnityEngine.Random.Range(0, GetAvailableColors().Length);
        int minimumPossibleScoreFound = 10000000;
        int minimumScoreIndex = UnityEngine.Random.Range(0, GetAvailableColors().Length);
        int startingIndex = UnityEngine.Random.Range(0, GetAvailableColors().Length);

        for (int i = startingIndex; i < GetAvailableColors().Length + startingIndex; i++) {
            int index = i;
            if (index >= GetAvailableColors().Length) {
                index = i - GetAvailableColors().Length;
            }
            Color originalColor = positions[collum, row].GetElementValue();
            if (index >= GetAvailableColors().Length || index < 0) {
                Debug.LogError("Index out of range. StartingIndex: " + startingIndex + ", currentIndex: " + index);
            }
            positions[collum, row].OnElementAppearance(GetAvailableColors() [index]);

            int potentialScore = isPotentialInput(collum, row, positions, matchedElemPositions, totalCollums, totalRows);
            //Debug.Log("### Potential score: " + potentialScore);
            positions[collum, row].OnElementAppearance(originalColor);
            if (potentialScore <= maxScoreAllowed && potentialScore > bestPossibleScoreFound) {
                bestPossibleScoreFound = potentialScore;
                bestScoreIndex = index;
            }
            else if (minimumPossibleScoreFound > potentialScore) {
                minimumScoreIndex = index;
                minimumPossibleScoreFound = potentialScore;
            }
        }
        if (bestPossibleScoreFound > 0 && !forceGetMin) {
            //Debug.Log("### SCORE ### Best score found: " + bestPossibleScoreFound + " for element " + positions[collum, row].GetAttachedGameObject().transform.parent.GetChild(bestScoreIndex));
            currentScorePossible = bestPossibleScoreFound;
            return GetAvailableColors() [bestScoreIndex];
        }
        else {
            //Debug.Log("### SCORE ### Worst score found: " + minimumPossibleScoreFound + " for element " + positions[collum, row].GetAttachedGameObject().transform.parent.GetChild(bestScoreIndex));
            currentScorePossible = minimumPossibleScoreFound;
            return GetAvailableColors() [minimumScoreIndex];
        }
    }

    public static Color[] GetAvailableColors() {
        return availColors;
    }
    /// <summary> Highlights a cell by enabling the panel of it's child. Used to visualize correct input </summary>
    public static void ToggleHighlightCell(int collum, int row, BoardElement[, ] positions, bool shouldHighlight, Color highlightColor) {
        if (shouldHighlight) {
            positions[collum, row].GetAttachedGameObject().transform.GetChild(1).gameObject.GetComponent<UnityEngine.UI.Image>().color = highlightColor;
            positions[collum, row].GetAttachedGameObject().transform.GetChild(1).gameObject.SetActive(true);
        }
        else {
            positions[collum, row].GetAttachedGameObject().transform.GetChild(1).gameObject.SetActive(false);
        }
    }

    public static BoardElement GetElementBasedOnParentIndex(BoardElement[, ] positions, int index) {
        for (int row = 0; row < positions.GetLength(1); row++) {
            for (int collum = 0; collum < positions.GetLength(0); collum++) {
                if (positions[collum, row].GetChildIndex() == index) {
                    return positions[collum, row];
                }
            }
        }
        Debug.LogError("Could not find element with parent index: " + index);
        return null;
    }

    public static BoardElement CreateNewElement(BoardElement previousElement, System.Type boardElementType) {

        if (boardElementType == typeof(CashBoardElement)) {
            previousElement = new CashBoardElement(previousElement.GetAttachedGameObject(), previousElement.GetChildIndex(), FixedElementData.cashElementValue);
        }
        else if (boardElementType == typeof(CrossBoardElement)) {
            Color newColor = BoardFunctions.availColors[Random.Range(0, BoardFunctions.availColors.Length)];
            previousElement = new CrossBoardElement(previousElement.GetAttachedGameObject(), previousElement.GetChildIndex(), previousElement.GetElementValue());
        }
        else if (boardElementType == typeof(BombBoardElement)) {
            previousElement = new BombBoardElement(previousElement.GetAttachedGameObject(), previousElement.GetChildIndex());
        }
        else if (boardElementType == typeof(BellBoardElement)) {
            previousElement = new BellBoardElement(previousElement.GetAttachedGameObject(), previousElement.GetChildIndex());
        }
        else {
            if (previousElement.GetElementClassType() != typeof(BoardElement)) {
                Color newColor = BoardFunctions.availColors[Random.Range(0, BoardFunctions.availColors.Length)];
                previousElement = new BoardElement(previousElement.GetAttachedGameObject(), previousElement.GetChildIndex(), newColor);
            }
            else {
                Color newColor = BoardFunctions.availColors[Random.Range(0, BoardFunctions.availColors.Length)];
                previousElement.OnElementAppearance(newColor);
            }
        }
        return previousElement;
    }

    public static bool DestroyBoardElement(int collum, int row, ref BoardElement[, ] positions, ref bool[, ] matchedElemPositions, ref List<AnimationMessage> playingAnimations, BoardElement lastElementProccesed) {
        AlexDebugger.GetInstance().AddMessage("Element: " + positions[collum, row].GetAttachedGameObject().transform.name + "  was a match of color: " + positions[collum, row].GetElementValue().ToString() + ", de-activating highlight.", AlexDebugger.tags.Step2);
        if (positions[collum, row].GetElementClassType() == typeof(BombBoardElement) || positions[collum, row].GetElementClassType() == typeof(CrossBoardElement)) {
            if (positions[collum, row].OnElementDestruction(positions, ref matchedElemPositions)) {
                return true;
            }
        }
        else if (positions[collum, row].GetElementClassType() == typeof(BellBoardElement)) {
            if (positions[collum, row].OnElementDestruction(ref positions, ref matchedElemPositions, ref playingAnimations, lastElementProccesed)) {
                return true;
            }
        }
        else {
            if (positions[collum, row].OnElementDestruction(positions)) {
                return true;
            }
        }
        return false;
    }

    public static void DestroyAllElementsCrossStyle(int crossElementCollum, int crossElementRow, ref bool[, ] matchedElements) {
        for (int row = 0; row < matchedElements.GetLength(1); row++) {
            matchedElements[crossElementCollum, row] = true;
        }
        for (int collum = 0; collum < matchedElements.GetLength(0); collum++) {
            matchedElements[collum, crossElementRow] = true;
        }
    }
    public static void DestroyElementsBombStyle(int bombElementCollum, int bombElementRow, ref bool[, ] matchedElements) {
        for (int row = bombElementRow - 2; row <= bombElementRow + 2; row++) {
            if (row < 0 || row >= matchedElements.GetLength(1)) {
                continue;
            }
            matchedElements[bombElementCollum, row] = true;
            if (row == bombElementRow - 1 || row == bombElementRow + 1) {
                if (bombElementRow - 1 > -1 && bombElementRow + 1 < matchedElements.GetLength(1)) {
                    if (bombElementCollum - 1 > -1) {
                        matchedElements[bombElementCollum - 1, row] = true;
                    }
                    if (bombElementCollum + 1 < matchedElements.GetLength(0)) {
                        matchedElements[bombElementCollum + 1, row] = true;
                    }
                }
            }
        }
        for (int collum = bombElementCollum - 2; collum <= bombElementCollum + 2; collum++) {
            if (collum < 0 || collum >= matchedElements.GetLength(0)) {
                continue;
            }
            matchedElements[collum, bombElementRow] = true;
        }
    }
    public static void DestroyElementsCrossBombStyle(int bombElementCollum, int bombElementRow, ref bool[, ] matchedElements) {

        for (int row = bombElementRow - 2; row <= bombElementRow + 2; row++) {
            for (int collum = bombElementCollum - 2; collum <= bombElementCollum + 2; collum++) {
                if (row < 0 || row >= matchedElements.GetLength(1)) {
                    continue;
                }
                if (collum < 0 || collum >= matchedElements.GetLength(0)) {
                    continue;
                }
                matchedElements[collum, row] = true;
            }
        }
    }

    public static void DestroyElementsDoubleBombStyle(int bombElementCollum, int bombElementRow, ref bool[, ] matchedElements) {
        if (bombElementRow - 3 > 0) {
            matchedElements[bombElementCollum, bombElementRow - 3] = true;
        }
        if (bombElementRow + 3 < matchedElements.GetLength(1)) {
            matchedElements[bombElementCollum, bombElementRow + 3] = true;
        }
        if (bombElementCollum - 3 > 0) {
            matchedElements[bombElementCollum - 3, bombElementRow] = true;
        }
        if (bombElementCollum + 3 < matchedElements.GetLength(0)) {
            matchedElements[bombElementCollum + 3, bombElementRow] = true;
        }

        for (int row = bombElementRow - 2; row <= bombElementRow + 2; row++) {
            for (int collum = bombElementCollum - 2; collum <= bombElementCollum + 2; collum++) {
                if (row < 0 || row >= matchedElements.GetLength(1)) {
                    continue;
                }
                if (collum < 0 || collum >= matchedElements.GetLength(0)) {
                    continue;
                }
                matchedElements[collum, row] = true;
            }
        }
    }

    public static void ActivateBellFunction(ref BoardElement[, ] positions, ref bool[, ] matchedElements, Color valueOfElement, ref List<AnimationMessage> playingAnimations, BoardElement secondElement) {
        for (int row = 0; row < positions.GetLength(1); row++) {
            for (int collum = 0; collum < positions.GetLength(0); collum++) {
                if (secondElement.GetElementClassType() == typeof(BoardElement)) {
                    if (positions[collum, row].GetElementValue() == valueOfElement && positions[collum, row].GetElementClassType() == typeof(BoardElement)) {
                        playingAnimations.Add(new AnimationMessage(positions[collum, row].GetChildIndex(), AssetLoader.GetBellElementSprite(), Color.white));
                        positions[collum, row].OnElementDestruction(positions);
                        matchedElements[collum, row] = true;
                    }
                }
                else if (secondElement.GetElementClassType() == typeof(CrossBoardElement)) {
                    if (positions[collum, row].GetElementValue() == valueOfElement && positions[collum, row].GetElementClassType() == typeof(BoardElement)) {
                        positions[collum, row] = new CrossBoardElement(positions[collum, row].GetAttachedGameObject(), positions[collum, row].GetChildIndex(), positions[collum, row].GetElementValue());
                        playingAnimations.Add(new AnimationMessage(positions[collum, row].GetChildIndex(), AssetLoader.GetCrossElementSprite(), positions[collum, row].GetElementValue()));
                        positions[collum, row].OnElementDestruction(positions, ref matchedElements);
                        matchedElements[collum, row] = true;
                    }
                }
                else if (secondElement.GetElementClassType() == typeof(BombBoardElement)) {
                    if (positions[collum, row].GetElementClassType() == typeof(BombBoardElement)) {
                        positions[collum, row].OnElementDestruction(positions, ref matchedElements);
                        matchedElements[collum, row] = true;
                    }
                }
                else if (secondElement.GetElementClassType() == typeof(BellBoardElement)) {

                    playingAnimations.Add(new AnimationMessage(positions[collum, row].GetChildIndex(), AssetLoader.GetBellElementSprite(), Color.white));
                    if (positions[collum, row].GetElementClassType() != typeof(CashBoardElement)) {
                        positions[collum, row] = new BoardElement(positions[collum, row].GetAttachedGameObject(), positions[collum, row].GetChildIndex(), Color.white);
                    }
                    positions[collum, row].OnElementDestruction(positions);
                    matchedElements[collum, row] = true;
                }
            }
        }
    }

    public static bool GetChances(float chances) {
        if (chances >= Random.Range(0, 100)) {
            return true;
        }
        else {
            return false;
        }
    }

    public static bool GetIfMatchCreatesBell(int collum, int row, BoardElement[, ] positions, ref bool[, ] searchedElements) {
        if (!searchedElements[collum, row]) {

            int numberOfRightMatches = CheckRightNeighboursForMatches(collum, row, positions, searchedElements.GetLength(0));
            if (numberOfRightMatches >= 4) {
                for (int i = collum; i < collum + 4; i++) {
                    if (i < searchedElements.GetLength(0)) {
                        searchedElements[i, row] = true;
                    }

                }
                searchedElements[collum, row] = true;
                return true;
            }
            int numberOfBottomMatches = CheckBottomNeighboursForMatches(collum, row, positions, searchedElements.GetLength(1));
            if (numberOfBottomMatches >= 4) {
                for (int i = row; i < row + 4; i++) {
                    if (i < searchedElements.GetLength(1)) {
                        searchedElements[collum, i] = true;
                    }

                }
                searchedElements[collum, row] = true;
                return true;
            }
        }
        return false;
    }

    public static bool GetIfMatchCreatesBomb(int collum, int row, BoardElement[, ] positions, ref bool[, ] searchedElements) {
        if (!searchedElements[collum, row]) {
            int numberOfRightMatches = CheckRightNeighboursForMatches(collum, row, positions, searchedElements.GetLength(0));
            int numberOfLeftMatches = CheckLeftNeighboursForMatches(collum, row, positions);
            int numberOfBottomMatches = CheckBottomNeighboursForMatches(collum, row, positions, searchedElements.GetLength(1));
            int numberOfTopMatches = CheckUpperNeighboursForMatches(collum, row, positions);

            if (numberOfRightMatches >= 1 && numberOfLeftMatches >= 1 && numberOfBottomMatches >= 1) {
                Debug.LogError(positions[collum, row].GetAttachedGameObject().name + " created a bomb match");
                searchedElements[collum + 1, row] = true;
                searchedElements[collum - 1, row] = true;
                searchedElements[collum, row + 1] = true;
                searchedElements[collum, row + 2] = true;
                searchedElements[collum, row] = true;
                return true;

            }
            else if (numberOfRightMatches >= 2 && numberOfTopMatches >= 2) {
                searchedElements[collum + 1, row] = true;
                searchedElements[collum + 2, row] = true;
                searchedElements[collum, row + 1] = true;
                searchedElements[collum, row + 2] = true;
                searchedElements[collum, row] = true;
                return true;
            }
        }
        return false;
    }

    public static bool GetIfMatchCreatesCross(int collum, int row, BoardElement[, ] positions, ref bool[, ] searchedElements) {
        if (!searchedElements[collum, row]) {
            int numberOfRightMatches = CheckRightNeighboursForMatches(collum, row, positions, searchedElements.GetLength(0));
            if (numberOfRightMatches >= 3) {
                for (int i = collum; i < collum + 3; i++) {
                    if (i < searchedElements.GetLength(0)) {
                        searchedElements[i, row] = true;
                    }

                }
                searchedElements[collum, row] = true;
                return true;
            }
            int numberOfBottomMatches = CheckBottomNeighboursForMatches(collum, row, positions, searchedElements.GetLength(1));
            if (numberOfBottomMatches >= 3) {
                for (int i = row; i < row + 3; i++) {
                    if (i < searchedElements.GetLength(1)) {
                        searchedElements[collum, i] = true;
                    }

                }
                searchedElements[collum, row] = true;
                return true;
            }
        }
        return false;
    }
#region Debug
    public static void CheckIfElementAtPositionIsNull(int collum, int row, BoardElement[, ] positions) {
#if UNITY_EDITOR
        if (positions[collum, row] == null) {
            Debug.LogError("Position: (" + collum + ", " + row + "), is null");
        }
#endif
    }
#endregion
}