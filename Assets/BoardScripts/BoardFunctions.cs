    using System.Collections.Generic;
    using System.Numerics;
    using System;
    //using UnityEngine;

    public static class BoardFunctions {

        static System.Random random = new System.Random();

        /// <summary>Returns the position of the element on the board</summary>
        public static KeyValuePair<int, int> GetBoardPositionOfElement(BoardElement element, BoardElement[, ] positions) {
            for (int row = 0; row < positions.GetLength(1); row++) {
                for (int collumn = 0; collumn < positions.GetLength(0); collumn++) {
                    if (positions[collumn, row] == element) {
                        return new KeyValuePair<int, int>(collumn, row);
                    }
                }
            }
            //Debug.LogError("Could not find element: " + GetTransformByIndex(element.GetTransformIndex()) + " on board!");
            return new KeyValuePair<int, int>();
        }

        /// <summary>Swaps to elements positions on the board </summary>
        public static void SwapBoardElementNeighbours(BoardElement oldElement, BoardElement newElement, ref BoardElement[, ] positions) {

            KeyValuePair<int, int> oldElementPosition = BoardFunctions.GetBoardPositionOfElement(oldElement, positions);
            KeyValuePair<int, int> newElementPosition = BoardFunctions.GetBoardPositionOfElement(newElement, positions);

            positions[oldElementPosition.Key, oldElementPosition.Value] = newElement;
            positions[newElementPosition.Key, newElementPosition.Value] = oldElement;

#if UNITY_EDITOR
            //Debug.Log(newElementPosition.Key + ", " + newElementPosition.Value + " set to position " + oldElementPosition.Key + ", " + oldElementPosition.Value);
            //Debug.Log(oldElementPosition.Key + ", " + oldElementPosition.Value + " set to position " + newElementPosition.Key + ", " + newElementPosition.Value);
            if (positions[oldElementPosition.Key, oldElementPosition.Value] == positions[newElementPosition.Key, newElementPosition.Value]) {
                UnityEngine.Debug.LogError("Swap was not correct");
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
        public static void SwapElements(BoardElement firstElement, BoardElement secondElement, BoardManager board, bool rewire = false, float speed = -1) {
            if (speed == -1) {
                speed = ConstantValues.swappingSpeed;
            }
            KeyValuePair<int, int> firstElementPosOnBoard = GetBoardPositionOfElement(firstElement, board.elementsPositions);
            KeyValuePair<int, int> secondElementPosOnBoard = GetBoardPositionOfElement(secondElement, board.elementsPositions);

            board.currentMessageID += 1;
            board.messagesToClient.Add(new Messages.AnimationMessage(board.currentMessageID, -1, Messages.AnimationMessage.AnimationMessageTypes.MoveTo, firstElement.GetTransformIndex(), speed, secondElementPosOnBoard.Key, secondElementPosOnBoard.Value));

            board.currentMessageID += 1;
            board.messagesToClient.Add(new Messages.AnimationMessage(board.currentMessageID, -1, Messages.AnimationMessage.AnimationMessageTypes.MoveTo, secondElement.GetTransformIndex(), speed, firstElementPosOnBoard.Key, firstElementPosOnBoard.Value));

            if (!rewire) {
                BoardFunctions.SwapBoardElementNeighbours(firstElement, secondElement, ref board.elementsPositions);
            }
            else {

                board.currentMessageID += 1;
                board.messagesToClient.Add(new Messages.AnimationMessage(board.currentMessageID, board.currentMessageID - 2, Messages.AnimationMessage.AnimationMessageTypes.MoveTo, firstElement.GetTransformIndex(), speed, firstElementPosOnBoard.Key, firstElementPosOnBoard.Value));
                board.currentMessageID += 1;
                board.messagesToClient.Add(new Messages.AnimationMessage(board.currentMessageID, board.currentMessageID - 2, Messages.AnimationMessage.AnimationMessageTypes.MoveTo, secondElement.GetTransformIndex(), speed, secondElementPosOnBoard.Key, secondElementPosOnBoard.Value));

            }

        }

        /// <summary> Returns the number of elements matched towards top </summary>
        public static int CheckUpperNeighboursForMatches(int collum, int row, BoardElement[, ] positions) {
            int numberOfElem = 0;

            if (row - 1 > -1) {
                if (positions[collum, row - 1].GetElementClassType() != typeof(BoardElement) && positions[collum, row - 1].GetElementClassType() != typeof(CrossBoardElement)) {
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
                if (positions[collum, row + 1].GetElementClassType() != typeof(BoardElement) && positions[collum, row + 1].GetElementClassType() != typeof(CrossBoardElement)) {
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
                if (positions[collum - 1, row].GetElementClassType() != typeof(BoardElement) && positions[collum - 1, row].GetElementClassType() != typeof(CrossBoardElement)) {
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
                if (positions[collum + 1, row].GetElementClassType() != typeof(BoardElement) && positions[collum + 1, row].GetElementClassType() != typeof(CrossBoardElement)) {
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
        public static int CheckElementForMatches(int startCollum, int startRow, BoardElement[, ] positions, ref bool[, ] matchedElemPositions, int collumsNumber, int rowsNumber, bool shouldFlag = true) {

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
                        //AlexDebugger.GetInstance().AddMessage("Upper matches for element: " + GetTransformByIndex(positions[startCollum, startRow].GetTransformIndex()) + " are: " + upperMatches, AlexDebugger.tags.Matches);
                    }
                    if (bottomMatches > 0) {
                        //AlexDebugger.GetInstance().AddMessage("Bottom matches for element: " + GetTransformByIndex(positions[startCollum, startRow].GetTransformIndex()) + " are: " + bottomMatches, AlexDebugger.tags.Matches);
                    }
                }
                // if more than 2 element matches, add horizontal mathced elements
                if (leftMatches + rightMatches >= 2) {
                    matchedElemPositions[startCollum, startRow] = true;
                    for (int collum = startCollum - leftMatches; collum <= startCollum + rightMatches; collum++) {
                        matchedElemPositions[collum, startRow] = true;
                    }
                    if (leftMatches > 0) {
                        //AlexDebugger.GetInstance().AddMessage("Left matches for element: " + GetTransformByIndex(positions[startCollum, startRow].GetTransformIndex()) + " are: " + leftMatches, AlexDebugger.tags.Matches);
                    }
                    if (rightMatches > 0) {
                        //AlexDebugger.GetInstance().AddMessage("Bottom matches for element: " + GetTransformByIndex(positions[startCollum, startRow].GetTransformIndex()) + " are: " + rightMatches, AlexDebugger.tags.Matches);
                    }
                }
                if (upperMatches + bottomMatches >= 2 || leftMatches + rightMatches >= 2) {
                    //AlexDebugger.GetInstance().AddMessage("Total matches found for: " + GetTransformByIndex(positions[startCollum, startRow].GetTransformIndex()) + " are: " + (upperMatches + bottomMatches + leftMatches + rightMatches).ToString(), AlexDebugger.tags.Matches);
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
        public static bool MoveMatchedElementUpwards(int collum, int row, BoardManager board) {
            bool didSwapOccur = false;
            //AlexDebugger.GetInstance().AddMessage("Moving upwards " + boardInstance.elementsPositions[collumn, row].GetAttachedGameObject().name + " from position: " + collumn + ", " + row, AlexDebugger.tags.UpwardMovement);
            // swap with next non matched element on the collum if not on top of collum
            int newRow = row;
            if (row == 0) {
                // if element reached top or there are no non-matched elements above, mark this element as destroyed and non-matched
                //board.positionsDestroyed[collum, newRow] = true;
                board.matchedElemPositions[collum, newRow] = false;
                return false;
            }
            Queue<int> destroyedElementsInRow = new Queue<int>();
            if (board.matchedElemPositions[collum, row]) {
                destroyedElementsInRow.Enqueue(row);
            }
            for (int i = row - 1; i > -1; i--) {
                // if non-matched element found above, swap
                if (board.matchedElemPositions[collum, i] == false && destroyedElementsInRow.Count > 0) {
                    //AlexDebugger.GetInstance().AddMessage("Swapping " + boardInstance.elementsPositions[collumn, row].GetAttachedGameObject().name + " with " + boardInstance.elementsPositions[collumn, i].GetAttachedGameObject().name + " at position " + collumn + ", " + i, AlexDebugger.tags.UpwardMovement);

                    int nextEmptyPosition = destroyedElementsInRow.Dequeue();
                    BoardFunctions.SwapElements(board.elementsPositions[collum, nextEmptyPosition], board.elementsPositions[collum, i], board, false, ConstantValues.swappingSpeed * 2.5f);
                    board.matchedElemPositions[collum, nextEmptyPosition] = false;
                    board.changedPotitions[collum, nextEmptyPosition] = true;
                    board.matchedElemPositions[collum, i] = true;
                    destroyedElementsInRow.Enqueue(i);
                    didSwapOccur = true;

                }
                else if (board.matchedElemPositions[collum, i] == true) {
                    destroyedElementsInRow.Enqueue(i);
                }
            }

            return didSwapOccur;

        }

        private static BoardElement GetNewElementForPosition(int collum, int row, BoardManager board, ref bool[, ] searchedElements) {
            if (BoardFunctions.GetIfMatchCreatesBell(collum, row, board.elementsPositions, ref searchedElements)) {
                return BoardFunctions.CreateNewElement(board.elementsPositions[collum, row], BoardElement.BoardElementTypes.Bell);
            }
            else if (BoardFunctions.GetIfMatchCreatesBomb(collum, row, board.elementsPositions, ref searchedElements)) {
                return BoardFunctions.CreateNewElement(board.elementsPositions[collum, row], BoardElement.BoardElementTypes.Bomb);
            }
            else if (BoardFunctions.GetIfMatchCreatesCross(collum, row, board.elementsPositions, ref searchedElements)) {
                return BoardFunctions.CreateNewElement(board.elementsPositions[collum, row], BoardElement.BoardElementTypes.Cross);
            }
            else {
                int cashElementIndex = BoardFunctions.GetIfCashElement();
                if (cashElementIndex > -1) {
                    return BoardFunctions.CreateNewCashElement(board.elementsPositions[collum, row], cashElementIndex);
                }
                else {
                    return BoardFunctions.CreateNewElement(board.elementsPositions[collum, row], BoardElement.BoardElementTypes.Default);
                }
            }
        }
        /// <summary>Re-creates destroyed elements, by moving them to holders position and then making a drop effect</summary>
        /// <returns>The best score that can be reached if swapped with neighbour</returns>
        public static void ReplaceElementAnimations(int collum, int row, BoardManager board, ref bool[, ] searchedElements) {

            //CheckIfElementAtPositionIsNull(collum, row, board.elementsPositions);

            //AlexDebugger.GetInstance().AddMessage("Replacing " + board.elementsPositions[collum, row], AlexDebugger.tags.Effects);

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
            board.elementsPositions[collum, row] = GetNewElementForPosition(collum, row, board, ref searchedElements);
            board.currentMessageID += 1;
            board.messagesToClient.Add(new Messages.AnimationMessage(board.currentMessageID, -1, Messages.AnimationMessage.AnimationMessageTypes.ChangeSprite, board.elementsPositions[collum, row].GetImageTransformIndex(), board.elementsPositions[collum, row].GetElementSpriteIndex(), board.elementsPositions[collum, row].GetElementValue()));
            board.currentMessageID += 1;
            board.messagesToClient.Add(new Messages.AnimationMessage(board.currentMessageID, -1, Messages.AnimationMessage.AnimationMessageTypes.MoveToHolder, board.elementsPositions[collum, row].GetTransformIndex(), 100000, collum, 0));
            board.currentMessageID += 1;
            board.messagesToClient.Add(new Messages.AnimationMessage(board.currentMessageID, board.currentMessageID - 1, Messages.AnimationMessage.AnimationMessageTypes.Scale, board.elementsPositions[collum, row].GetTransformIndex(), 100000, 1, 1, 1));
            board.currentMessageID += 1;
            board.messagesToClient.Add(new Messages.AnimationMessage(board.currentMessageID, board.currentMessageID - 1, Messages.AnimationMessage.AnimationMessageTypes.MoveTo, board.elementsPositions[collum, row].GetTransformIndex(), ConstantValues.swappingSpeed * 2.5f, collum, row));

            return;

        }

        /// <summary>Play scale-to-zero effect on element's transform</summary>
        public static void PlayMatchEffectAnimations(int collum, int row, BoardManager board) {
            //AlexDebugger.GetInstance().AddMessage("####### Scale to zero effect for " + positions[collum, row].GetAttachedGameObject().name, AlexDebugger.tags.Effects);
            board.currentMessageID += 1;
            board.messagesToClient.Add(new Messages.AnimationMessage(board.currentMessageID, -1, Messages.AnimationMessage.AnimationMessageTypes.Scale, board.elementsPositions[collum, row].GetTransformIndex(), ConstantValues.scaleToZeroAnimationSpeed, 0, 0, 0));
        }

        /// <summary>Check if with input it can create matches. Returns best score found </summary>
        public static int IsPotentialInput(int collum, int row, BoardElement[, ] positions, bool[, ] matchedElemPositions, int totalCollums, int totalRows) {
            int possibleScore = 0;
            if (row - 1 > -1 && positions[collum, row - 1].GetElementClassType() != typeof(CashBoardElement)) {
                BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum, row - 1], ref positions);
                int matches = BoardFunctions.CheckElementForMatches(collum, row - 1, positions, ref matchedElemPositions, totalCollums, totalRows, false);
                if (matches > 0) {
                    possibleScore = matches;
                }
                BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum, row - 1], ref positions);
            }
            if (row + 1 < totalRows && positions[collum, row + 1].GetElementClassType() != typeof(CashBoardElement)) {
                BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum, row + 1], ref positions);
                int matches = BoardFunctions.CheckElementForMatches(collum, row + 1, positions, ref matchedElemPositions, totalCollums, totalRows, false);
                if (matches > 0 && possibleScore < matches) {
                    possibleScore = matches;
                }
                BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum, row + 1], ref positions);
            }
            if (collum - 1 > -1 && positions[collum - 1, row].GetElementClassType() != typeof(CashBoardElement)) {
                BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum - 1, row], ref positions);
                int matches = BoardFunctions.CheckElementForMatches(collum - 1, row, positions, ref matchedElemPositions, totalCollums, totalRows, false);
                if (matches > 0 && possibleScore < matches) {
                    possibleScore = matches;
                }
                BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum - 1, row], ref positions);
            }
            if (collum + 1 < totalCollums && positions[collum + 1, row].GetElementClassType() != typeof(CashBoardElement)) {
                BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum + 1, row], ref positions);
                int matches = BoardFunctions.CheckElementForMatches(collum + 1, row, positions, ref matchedElemPositions, totalCollums, totalRows, false);
                if (matches > 0 && possibleScore < matches) {
                    possibleScore = matches;
                }
                BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum + 1, row], ref positions);
            }
            return possibleScore;
        }

        /// <summary> Returns if two elements are neighbours</summary>
        public static bool GetIfNeighbours(BoardElement elem1, BoardElement elem2, BoardElement[, ] positions) {
            KeyValuePair<int, int> elem1Pos = BoardFunctions.GetBoardPositionOfElement(elem1, positions);
            KeyValuePair<int, int> elem2Pos = BoardFunctions.GetBoardPositionOfElement(elem2, positions);
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
        // public static Color GetColorInScore(int collum, int row, ref BoardElement[, ] positions, bool[, ] matchedElemPositions, int totalCollums, int totalRows, int maxScoreAllowed, ref int currentScorePossible, bool forceGetMin) {
        //     int bestPossibleScoreFound = 0;
        //     int bestScoreIndex = UnityEngine.Random.Range(0, GetAvailableColors().Length);
        //     int minimumPossibleScoreFound = 10000000;
        //     int minimumScoreIndex = UnityEngine.Random.Range(0, GetAvailableColors().Length);
        //     int startingIndex = UnityEngine.Random.Range(0, GetAvailableColors().Length);

        //     for (int i = startingIndex; i < GetAvailableColors().Length + startingIndex; i++) {
        //         int index = i;
        //         if (index >= GetAvailableColors().Length) {
        //             index = i - GetAvailableColors().Length;
        //         }
        //         Color originalColor = positions[collum, row].GetElementValue();
        //         if (index >= GetAvailableColors().Length || index < 0) {
        //             Debug.LogError("Index out of range. StartingIndex: " + startingIndex + ", currentIndex: " + index);
        //         }
        //         positions[collum, row].OnElementAppearance(GetAvailableColors() [index]);

        //         int potentialScore = IsPotentialInput(collum, row, positions, matchedElemPositions, totalCollums, totalRows);
        //         //Debug.Log("### Potential score: " + potentialScore);
        //         positions[collum, row].OnElementAppearance(originalColor);
        //         if (potentialScore <= maxScoreAllowed && potentialScore > bestPossibleScoreFound) {
        //             bestPossibleScoreFound = potentialScore;
        //             bestScoreIndex = index;
        //         }
        //         else if (minimumPossibleScoreFound > potentialScore) {
        //             minimumScoreIndex = index;
        //             minimumPossibleScoreFound = potentialScore;
        //         }
        //     }
        //     if (bestPossibleScoreFound > 0 && !forceGetMin) {
        //         //Debug.Log("### SCORE ### Best score found: " + bestPossibleScoreFound + " for element " + positions[collum, row].GetAttachedGameObject().transform.parent.GetChild(bestScoreIndex));
        //         currentScorePossible = bestPossibleScoreFound;
        //         return GetAvailableColors() [bestScoreIndex];
        //     }
        //     else {
        //         //Debug.Log("### SCORE ### Worst score found: " + minimumPossibleScoreFound + " for element " + positions[collum, row].GetAttachedGameObject().transform.parent.GetChild(bestScoreIndex));
        //         currentScorePossible = minimumPossibleScoreFound;
        //         return GetAvailableColors() [minimumScoreIndex];
        //     }
        // }

        /// <summary> Highlights a cell by enabling the panel of it's child. Used to visualize correct input </summary>
        public static void ToggleHighlightCell(int collum, int row, BoardManager board, bool shouldHighlight) {

            board.currentMessageID += 1;
            if (shouldHighlight) {
                board.messagesToClient.Add(new Messages.AnimationMessage(board.currentMessageID, -1, Messages.AnimationMessage.AnimationMessageTypes.ChangeSprite, board.elementsPositions[collum, row].GetHighlightImageTransformIndex(), (int) ConstantValues.AvailableSprites.highlight));
            }
            else {
                board.messagesToClient.Add(new Messages.AnimationMessage(board.currentMessageID, -1, Messages.AnimationMessage.AnimationMessageTypes.ChangeSprite, board.elementsPositions[collum, row].GetHighlightImageTransformIndex(), (int) ConstantValues.AvailableSprites.transparent));
            }
        }

        public static BoardElement GetElementBasedOnParentIndex(BoardElement[, ] positions, int index) {
            for (int row = 0; row < positions.GetLength(1); row++) {
                for (int collum = 0; collum < positions.GetLength(0); collum++) {
                    if (positions[collum, row].GetTransformIndex() [positions[collum, row].GetTransformIndex().Length - 1] == index) {
                        return positions[collum, row];
                    }
                }
            }
            UnityEngine.Debug.LogError("Could not find element with parent index: " + index);
            return null;
        }

        public static BoardElement CreateNewElement(BoardElement previousElement, BoardElement.BoardElementTypes type) {
            int randomNum = random.Next(0, ConstantValues.GetAvailableColors().Length);
            Vector4 newColor = ConstantValues.GetAvailableColors() [randomNum];
            switch (type) {
                case BoardElement.BoardElementTypes.Cross:
                    previousElement = new CrossBoardElement(previousElement.GetTransformIndex(), previousElement.GetElementValue());
                    break;
                case BoardElement.BoardElementTypes.Bomb:
                    previousElement = new BombBoardElement(previousElement.GetTransformIndex());
                    break;
                case BoardElement.BoardElementTypes.Bell:
                    previousElement = new BellBoardElement(previousElement.GetTransformIndex());
                    break;
                case BoardElement.BoardElementTypes.Default:
                    if (previousElement.GetElementClassType() != typeof(BoardElement)) {
                        previousElement = new BoardElement(previousElement.GetTransformIndex(), newColor);
                    }
                    else {
                        previousElement.OnElementAppearance(newColor);
                    }
                    break;
            }

            return previousElement;
        }
        public static BoardElement CreateNewCashElement(BoardElement previousElement, int cashTypeIndex) {
            previousElement = new CashBoardElement(previousElement.GetTransformIndex(), cashTypeIndex);
            return previousElement;

        }

        public static bool DestroyBoardElement(int collum, int row, BoardManager board, BoardElement lastElementProccesed) {
            //AlexDebugger.GetInstance().AddMessage("Element: " + GetTransformByIndex(board.elementsPositions[collum, row].GetTransformIndex()) + "  was a match of color: " + board.elementsPositions[collum, row].GetElementValue().ToString() + ", de-activating highlight.", AlexDebugger.tags.Step2);
            if (board.elementsPositions[collum, row].GetElementClassType() == typeof(BombBoardElement) || board.elementsPositions[collum, row].GetElementClassType() == typeof(CrossBoardElement)) {
                if (board.elementsPositions[collum, row].OnElementDestruction(board)) {
                    return true;
                }
            }
            else if (board.elementsPositions[collum, row].GetElementClassType() == typeof(BellBoardElement)) {
                if (board.elementsPositions[collum, row].OnElementDestruction(board, lastElementProccesed)) {
                    return true;
                }
            }
            else if (board.elementsPositions[collum, row].GetElementClassType() == typeof(CashBoardElement)) {
                board.currentMessageID += 1;
                board.messagesToClient.Add(new Messages.AnimationMessage.ScrollWinHistoryMessage(board.currentMessageID, board.currentMessageID - 1, board.elementsPositions[collum, row].GetTransformIndex(), board.elementsPositions[collum, row].GetElementSpriteIndex(), ((CashBoardElement) board.elementsPositions[collum, row]).GetCashValue().ToString()));
                if (board.elementsPositions[collum, row].OnElementDestruction(board)) {
                    return true;
                }
            }
            else {
                if (board.elementsPositions[collum, row].OnElementDestruction(board)) {
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

        public static void ActivateBellFunction(BoardManager board, BoardElement secondElement) {
            for (int row = 0; row < board.elementsPositions.GetLength(1); row++) {
                for (int collum = 0; collum < board.elementsPositions.GetLength(0); collum++) {
                    int elementIndex = board.elementsPositions[collum, row].GetTransformIndex() [board.elementsPositions[collum, row].GetTransformIndex().Length - 1];
                    if (secondElement.GetElementClassType() == typeof(BoardElement)) {
                        if (board.elementsPositions[collum, row].GetElementValue() == secondElement.GetElementValue() && board.elementsPositions[collum, row].GetElementClassType() == typeof(BoardElement)) {
                            board.currentMessageID += 1;
                            board.messagesToClient.Add(new Messages.AnimationMessage(board.currentMessageID, -1, Messages.AnimationMessage.AnimationMessageTypes.ChangeSprite, board.elementsPositions[collum, row].GetImageTransformIndex(), (int) ConstantValues.AvailableSprites.bell, Vector4.One));
                            board.elementsPositions[collum, row].OnElementDestruction(board);
                            board.matchedElemPositions[collum, row] = true;
                        }
                    }
                    else if (secondElement.GetElementClassType() == typeof(CrossBoardElement)) {
                        if (board.elementsPositions[collum, row].GetElementValue() == secondElement.GetElementValue() && board.elementsPositions[collum, row].GetElementClassType() == typeof(BoardElement)) {
                            board.elementsPositions[collum, row] = new CrossBoardElement(board.elementsPositions[collum, row].GetTransformIndex(), board.elementsPositions[collum, row].GetElementValue());
                            board.currentMessageID += 1;
                            board.messagesToClient.Add(new Messages.AnimationMessage(board.currentMessageID, -1, Messages.AnimationMessage.AnimationMessageTypes.ChangeSprite, board.elementsPositions[collum, row].GetImageTransformIndex(), (int) ConstantValues.AvailableSprites.cross, board.elementsPositions[collum, row].GetElementValue()));
                            board.elementsPositions[collum, row].OnElementDestruction(board);
                            board.matchedElemPositions[collum, row] = true;
                        }
                    }
                    else if (secondElement.GetElementClassType() == typeof(BombBoardElement)) {
                        if (board.elementsPositions[collum, row].GetElementClassType() == typeof(BombBoardElement)) {
                            board.elementsPositions[collum, row].OnElementDestruction(board);
                            board.matchedElemPositions[collum, row] = true;
                        }
                    }
                    else if (secondElement.GetElementClassType() == typeof(BellBoardElement)) {
                        board.currentMessageID += 1;
                        board.messagesToClient.Add(new Messages.AnimationMessage(board.currentMessageID, -1, Messages.AnimationMessage.AnimationMessageTypes.ChangeSprite, board.elementsPositions[collum, row].GetImageTransformIndex(), (int) ConstantValues.AvailableSprites.bell, Vector4.One));
                        if (board.elementsPositions[collum, row].GetElementClassType() != typeof(CashBoardElement)) {
                            board.elementsPositions[collum, row] = new BoardElement(board.elementsPositions[collum, row].GetTransformIndex(), Vector4.One);
                        }
                        //board.elementsPositions[collum, row].OnElementDestruction(board);
                        board.matchedElemPositions[collum, row] = true;
                    }
                }
            }
        }

        public static int GetIfCashElement() {

            int roll = random.Next(0, 100);
            for (int i = ConstantValues.cashElementsChances.Length - 1; i > -1; i--) {
                if (roll <= ConstantValues.cashElementsChances[i]) {
                    return i;
                }
            }
            return -1;
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
                int numberOfBottomMatches = 0;

                if (numberOfRightMatches >= 2) {
                    numberOfBottomMatches = CheckBottomNeighboursForMatches(collum + 1, row, positions, searchedElements.GetLength(1));
                    if (numberOfBottomMatches >= 2) {
                        searchedElements[collum + 1, row] = true;
                        searchedElements[collum + 2, row] = true;
                        searchedElements[collum, row + 1] = true;
                        searchedElements[collum, row + 2] = true;
                        searchedElements[collum, row] = true;
                        return true;
                    }

                }
                numberOfBottomMatches = 0;
                numberOfRightMatches = 0;
                numberOfBottomMatches = CheckBottomNeighboursForMatches(collum, row, positions, searchedElements.GetLength(1));

                if (numberOfBottomMatches >= 2) {
                    numberOfRightMatches = CheckRightNeighboursForMatches(collum, row + 2, positions, searchedElements.GetLength(0));
                    if (numberOfRightMatches >= 2) {
                        searchedElements[collum + 1, row + 2] = true;
                        searchedElements[collum + 2, row + 2] = true;
                        searchedElements[collum, row + 1] = true;
                        searchedElements[collum, row + 2] = true;
                        searchedElements[collum, row] = true;
                        return true;
                    }
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
        //         public static void CheckIfElementAtPositionIsNull(int collum, int row, BoardElement[, ] positions) {
        // #if UNITY_EDITOR
        //             if (positions[collum, row] == null) {
        //                 Debug.LogError("Position: (" + collum + ", " + row + "), is null");
        //             }
        // #endif
        //         }
#endregion
        // public static Transform GetTransformByIndex(int[] indexInHierarchy) {

        //     Transform trans = GameObject.Find("Canvas").transform;

        //     for (int y = 0; y < indexInHierarchy.Length; y++) {
        //         // Debug.Log(message.type);
        //         // Debug.Log("index: " + y);
        //         // Debug.Log(trans.name);
        //         trans = trans.GetChild(indexInHierarchy[y]);
        //     }
        //     //Debug.Log("End");
        //     return trans;

        // }
    }