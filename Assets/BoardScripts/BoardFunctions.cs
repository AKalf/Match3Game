using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BoardFunctions
{

    /// <summary> Possible values of cells</summary>
    public static Color[] availColors = { Color.red, Color.blue, Color.green, Color.cyan, Color.magenta };

    /// <summary>Returns the position of the element on the board</summary>
    public static KeyValuePair<int, int> GetPositionOfElement(BoardElement element, BoardElement[,] positions)
    {
        for (int row = 0; row < positions.GetLength(1); row++)
        {
            for (int collumn = 0; collumn < positions.GetLength(0); collumn++)
            {
                if (positions[collumn, row] == element)
                {
                    return new KeyValuePair<int, int>(collumn, row);
                }
            }
        }
        Debug.LogError("Could not find element: " + element.GetAttachedGameObject().name + " on board!");
        return new KeyValuePair<int, int>();
    }

    /// <summary>Swaps to elements positions on the board </summary>
    public static void SwapBoardElementNeighbours(BoardElement oldElement, BoardElement newElement, ref BoardElement[,] positions)
    {
        KeyValuePair<int, int> oldElementPosition = BoardFunctions.GetPositionOfElement(oldElement, positions);
        KeyValuePair<int, int> newElementPosition = BoardFunctions.GetPositionOfElement(newElement, positions);

        positions[oldElementPosition.Key, oldElementPosition.Value] = newElement;
        positions[newElementPosition.Key, newElementPosition.Value] = oldElement;
#if UNITY_EDITOR
        //Debug.Log(newElementPosition.Key + ", " + newElementPosition.Value + " set to position " + oldElementPosition.Key + ", " + oldElementPosition.Value);
        //Debug.Log(oldElementPosition.Key + ", " + oldElementPosition.Value + " set to position " + newElementPosition.Key + ", " + newElementPosition.Value);
        if (positions[oldElementPosition.Key, oldElementPosition.Value] == positions[newElementPosition.Key, newElementPosition.Value])
        {
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
    public static void SwapElements(BoardElement firstElement, BoardElement secondElement, ref BoardElement[,] positions, ref List<AnimationMessage> playingAnimations, bool rewire = false, float swappingSpeed = 0.2f)
    {

        //Animations.Animation firstElementMoveAnim = Animations.MoveToPosition(firstElement.transform, swappingSpeed, secondElement.transform.position);
        Vector3 targetPos = secondElement.GetAttachedGameObject().transform.position;
        playingAnimations.Add(new AnimationMessage(Animations.AnimationTypes.MoveTo, firstElement.GetChildIndex(), swappingSpeed, targetPos.x, targetPos.y, targetPos.z));

        targetPos = firstElement.GetAttachedGameObject().transform.position;
        //Animations.Animation secondElementMoveAnim = Animations.MoveToPosition(secondElement.transform, swappingSpeed, firstElement.transform.position);
        playingAnimations.Add(new AnimationMessage(Animations.AnimationTypes.MoveTo, secondElement.GetChildIndex(), swappingSpeed, targetPos.x, targetPos.y, targetPos.z));

        if (!rewire)
        {
            BoardFunctions.SwapBoardElementNeighbours(firstElement, secondElement, ref positions);
        }
        else
        {
            targetPos = firstElement.GetAttachedGameObject().transform.position;
            playingAnimations.Add(new AnimationMessage(Animations.AnimationTypes.MoveTo, firstElement.GetChildIndex(), swappingSpeed, targetPos.x, targetPos.y, targetPos.z));
            targetPos = secondElement.GetAttachedGameObject().transform.position;
            playingAnimations.Add(new AnimationMessage(Animations.AnimationTypes.MoveTo, secondElement.GetChildIndex(), swappingSpeed, targetPos.x, targetPos.y, targetPos.z));
        }
        // if (playingAnimations.Count > 0) {
        //     Animations.SetAnimationMessages(playingAnimations.ToArray());
        //     playingAnimations.Clear();
        //     areAnimationsPlaying = true;
        // }
    }

    /// <summary> Returns the number of elements matched towards top </summary>
    public static int CheckUpperNeighboursForMatches(int collum, int row, BoardElement[,] positions)
    {
        int numberOfElem = 0;
        if (row - 1 > -1)
        {
            if (positions[collum, row].GetElementValue() == positions[collum, row - 1].GetElementValue())
            {
                numberOfElem++;
                numberOfElem += BoardFunctions.CheckUpperNeighboursForMatches(collum, row - 1, positions);
            }
        }
        return numberOfElem;
    }
    /// <summary> Returns the number of elements matched towards bottom </summary>
    public static int CheckBottomNeighboursForMatches(int collum, int row, BoardElement[,] positions, int rowsNumbers)
    {
        int numberOfElem = 0;
        if (row + 1 < rowsNumbers)
        {
            if (positions[collum, row].GetElementValue() == positions[collum, row + 1].GetElementValue())
            {
                numberOfElem++;
                numberOfElem += BoardFunctions.CheckBottomNeighboursForMatches(collum, row + 1, positions, rowsNumbers);

            }
        }
        return numberOfElem;

    }
    /// <summary> Returns the number of elements matched towards left </summary>
    public static int CheckLeftNeighboursForMatches(int collum, int row, BoardElement[,] positions)
    {
        int numberOfElem = 0;
        if (collum - 1 > -1)
        {
            if (positions[collum, row].GetElementValue() == positions[collum - 1, row].GetElementValue())
            {
                numberOfElem++;
                numberOfElem += BoardFunctions.CheckLeftNeighboursForMatches(collum - 1, row, positions);

            }

        }
        return numberOfElem;
    }
    /// <summary> Returns the number of elements matched towards right </summary>
    public static int CheckRightNeighboursForMatches(int collum, int row, BoardElement[,] positions, int collumsNumber)
    {
        int numberOfElem = 0;
        if (collum + 1 < collumsNumber)
        {
            if (positions[collum, row].GetElementValue() == positions[collum + 1, row].GetElementValue())
            {
                numberOfElem++;
                numberOfElem += BoardFunctions.CheckRightNeighboursForMatches(collum + 1, row, positions, collumsNumber);

            }
        }
        return numberOfElem;
    }

    /// <summary> Flags positions that belong to a match </summary>
    /// <param name="shouldFlag"> if false, elements wont be marked as matched (usefull when want to check possible matches)</param>
    /// <returns>returns the number of matches found</returns>
    public static int CheckForMatches(int startCollum, int startRow, BoardElement[,] positions, ref bool[,] matchedElemPositions, int collumsNumber, int rowsNumber, bool shouldFlag = true)
    {

        int upperMatches = BoardFunctions.CheckUpperNeighboursForMatches(startCollum, startRow, positions);
        int bottomMatches = BoardFunctions.CheckBottomNeighboursForMatches(startCollum, startRow, positions, rowsNumber);
        int leftMatches = BoardFunctions.CheckLeftNeighboursForMatches(startCollum, startRow, positions);
        int rightMatches = BoardFunctions.CheckRightNeighboursForMatches(startCollum, startRow, positions, collumsNumber);

        if (shouldFlag)
        {
            // if more than 2 element matches, add vertical mathced elements
            if (upperMatches + bottomMatches >= 2)
            {
                matchedElemPositions[startCollum, startRow] = true;
                for (int row = startRow - upperMatches; row <= startRow + bottomMatches; row++)
                {
                    matchedElemPositions[startCollum, row] = true;
                    AlexDebugger.GetInstance().AddMessage(positions[startCollum, row].GetAttachedGameObject().name + "is an " + ((row < startRow) ? " on top" : "on bottom") + " match of " + positions[startCollum, row].GetAttachedGameObject().name, AlexDebugger.tags.Matches);
                }
                if (upperMatches > 0)
                {
                    AlexDebugger.GetInstance().AddMessage("Upper matches for element: " + positions[startCollum, startRow].GetAttachedGameObject().name + " are: " + upperMatches, AlexDebugger.tags.Matches);
                }
                if (bottomMatches > 0)
                {
                    AlexDebugger.GetInstance().AddMessage("Bottom matches for element: " + positions[startCollum, startRow].GetAttachedGameObject().name + " are: " + bottomMatches, AlexDebugger.tags.Matches);
                }
            }
            // if more than 2 element matches, add horizontal mathced elements
            if (leftMatches + rightMatches >= 2)
            {
                matchedElemPositions[startCollum, startRow] = true;
                for (int collum = startCollum - leftMatches; collum <= startCollum + rightMatches; collum++)
                {
                    matchedElemPositions[collum, startRow] = true;
                }
                if (leftMatches > 0)
                {
                    AlexDebugger.GetInstance().AddMessage("Left matches for element: " + positions[startCollum, startRow].GetAttachedGameObject().name + " are: " + leftMatches, AlexDebugger.tags.Matches);
                }
                if (rightMatches > 0)
                {
                    AlexDebugger.GetInstance().AddMessage("Bottom matches for element: " + positions[startCollum, startRow].GetAttachedGameObject().name + " are: " + rightMatches, AlexDebugger.tags.Matches);
                }
            }
        }
        if (upperMatches + bottomMatches >= 2 || leftMatches + rightMatches >= 2)
        {
            AlexDebugger.GetInstance().AddMessage("Total matches found for: " + positions[startCollum, startRow].GetAttachedGameObject().name + " are: " + upperMatches + bottomMatches + leftMatches + rightMatches, AlexDebugger.tags.Matches);
            return upperMatches + bottomMatches + leftMatches + rightMatches;
        }
        else
        {
            return 0;
        }
    }

    /// <summary> Used recursivly to move non-destroyed elements dowards</summary>
    public static void MoveMatchedElementUpwards(
        int collumn, int row,
        ref BoardElement[,] positions, ref bool[,] matchedElemPositions, ref bool[,] positionsDestroyed,
        ref List<AnimationMessage> playingAnimations, float swappingSpeed)
    {
        AlexDebugger.GetInstance().AddMessage("####### Moving upwards " + positions[collumn, row].GetAttachedGameObject().name + ", elements", AlexDebugger.tags.UpwardMovement);
        // swap with next non matched element on the collum if not on top of collum
        if (row != 0)
        {
            for (int i = row - 1; i > -1; i--)
            {
                // if non-matched element found, swap
                if (matchedElemPositions[collumn, i] == false)
                {
                    BoardFunctions.SwapElements(positions[collumn, row], positions[collumn, i], ref positions, ref playingAnimations, false, swappingSpeed);

                    // make changes to the flag array
                    matchedElemPositions[collumn, row] = false;
                    matchedElemPositions[collumn, i] = true;
                    return;
                }
            }
        }
        // if element reached top marked it as destroyed and non-matched
        positionsDestroyed[collumn, row] = true;
        matchedElemPositions[collumn, row] = false;
    }

    /// <summary>Re-creates destroyed elements, by moving them to holders position and then making a drop effect</summary>
    /// <returns>The best score that can be reached if swapped with neighbour</returns>
    public static int ReplaceElement(int collum, int row,
        ref BoardElement[,] positions, ref bool[,] matchedElemPositions,
        Transform[] holders,
        int maxScoreAllowed, int currentMaxScorePossible, float cashElementPossibility, int cashElementValue, int totalCollums, int totalRows,
        ref List<AnimationMessage> playingAnimations, float swappingSpeed)
    {
        CheckIfElementAtPositionIsNull(collum, row, positions);
        int maxScorePossible = 0;
        float cashElementChance = Random.Range(0, 100);
        if (cashElementPossibility >= cashElementChance)
        {
            positions[collum, row] = new CashBoardElement(positions[collum, row].GetAttachedGameObject(), positions[collum, row].GetChildIndex(), Color.white, cashElementValue);
        }
        else if (positions[collum, row].GetElementClassType() != typeof(BoardElement))
        {
            positions[collum, row] = new BoardElement(positions[collum, row].GetAttachedGameObject(), positions[collum, row].GetChildIndex(), Color.black);
        }
        Color newColor = GetColorInScore(collum, row, ref positions, matchedElemPositions, totalCollums, totalRows, maxScoreAllowed, ref maxScorePossible, (currentMaxScorePossible >= maxScoreAllowed - 2) ? true : false);
        positions[collum, row].OnElementCreation(newColor);

        if (maxScorePossible > maxScoreAllowed)
        {
            Debug.LogError("CurrentMaxScorePossible: " + maxScorePossible + " exceeds maxScoreAllowed: " + maxScorePossible);
        }
        AlexDebugger.GetInstance().AddMessage("Replacing " + positions[collum, row], AlexDebugger.tags.Effects);
        Vector3 originalPos = positions[collum, row].GetAttachedGameObject().transform.position;
        positions[collum, row].GetAttachedGameObject().transform.position = holders[collum].position;
        positions[collum, row].GetAttachedGameObject().transform.localScale = Vector3.one;
        playingAnimations.Add(new AnimationMessage(Animations.AnimationTypes.MoveTo, positions[collum, row].GetChildIndex(), 200, holders[collum].position.x, holders[collum].position.y, holders[collum].position.z));
        playingAnimations.Add(new AnimationMessage(Animations.AnimationTypes.ScaleToOne, positions[collum, row].GetChildIndex(), 200));
        playingAnimations.Add(new AnimationMessage(Animations.AnimationTypes.MoveTo, positions[collum, row].GetChildIndex(), swappingSpeed, originalPos.x, originalPos.y, originalPos.z));
        return maxScorePossible;

    }

    /// <summary>Play scale-to-zero effect on element's transform</summary>
    public static void PlayMatchEffect(int collum, int row, BoardElement[,] positions, ref List<AnimationMessage> playingAnimations, float swappingSpeed)
    {
        AlexDebugger.GetInstance().AddMessage("####### Scale to zero effect for " + positions[collum, row].GetAttachedGameObject().name, AlexDebugger.tags.Effects);
        //Animations.Animation scaleToZero = Animations.AddAnimationScaleToZero(positions[collum, row].transform, swappingSpeed);
        positions[collum, row].OnElementDestruction();
        playingAnimations.Add(new AnimationMessage(Animations.AnimationTypes.ScaleToZero, positions[collum, row].GetChildIndex(), swappingSpeed));

    }

    /// <summary>Checks each element on the board for matches and flags them</summary>
    public static int CheckBoardForMatches(BoardElement[,] positions, ref bool[,] matchedElemPositions, int collumsNumber, int rowsNumber)
    {
        AlexDebugger.GetInstance().AddMessage("####### Checking after-match...", AlexDebugger.tags.Aftermatch);
        int totalMatches = 0;
        // Search for matches and flag them
        for (int row = 0; row < positions.GetLength(1); row++)
        {
            for (int collum = 0; collum < positions.GetLength(0); collum++)
            {
                totalMatches += CheckForMatches(collum, row, positions, ref matchedElemPositions, collumsNumber, rowsNumber, true);
                if (row == positions.GetLength(1) - 1 && positions[collum, row].GetElementClassType() == typeof(CashBoardElement))
                {
                    matchedElemPositions[collum, row] = true;
                    totalMatches++;
                }
            }

        }
        return totalMatches;

    }

    /// <summary>Check if with input it can create matches. Returns best score found </summary>
    public static int isPotentialInput(int collum, int row, BoardElement[,] positions, bool[,] matchedElemPositions, int totalCollums, int totalRows)
    {
        int possibleScore = 0;
        if (row - 1 > -1)
        {
            BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum, row - 1], ref positions);
            int matches = BoardFunctions.CheckForMatches(collum, row - 1, positions, ref matchedElemPositions, totalCollums, totalRows, false);
            if (matches > 0)
            {
                BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum, row - 1], ref positions);
                possibleScore = matches;
            }
            else
            {
                BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum, row - 1], ref positions);
            }
        }
        if (row + 1 < totalRows)
        {
            BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum, row + 1], ref positions);
            int matches = BoardFunctions.CheckForMatches(collum, row + 1, positions, ref matchedElemPositions, totalCollums, totalRows, false);
            if (matches > 0)
            {
                BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum, row + 1], ref positions);
                if (possibleScore < matches)
                {
                    possibleScore = matches;
                }

            }
            else
            {
                BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum, row + 1], ref positions);
            }
        }
        if (collum - 1 > -1)
        {
            BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum - 1, row], ref positions);
            int matches = BoardFunctions.CheckForMatches(collum - 1, row, positions, ref matchedElemPositions, totalCollums, totalRows, false);
            if (matches > 0)
            {
                BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum - 1, row], ref positions);
                if (possibleScore < matches)
                {
                    possibleScore = matches;
                }
            }
            else
            {
                BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum - 1, row], ref positions);
            }
        }
        if (collum + 1 < totalCollums)
        {
            BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum + 1, row], ref positions);
            int matches = BoardFunctions.CheckForMatches(collum + 1, row, positions, ref matchedElemPositions, totalCollums, totalRows, false);
            if (matches > 0)
            {
                BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum + 1, row], ref positions);
                if (possibleScore < matches)
                {
                    possibleScore = matches;
                }
            }
            else
            {
                BoardFunctions.SwapBoardElementNeighbours(positions[collum, row], positions[collum + 1, row], ref positions);
            }
        }
        return possibleScore;
    }

    /// <summary> Returns if two elements are neighbours</summary>
    public static bool GetIfNeighbours(BoardElement elem1, BoardElement elem2, BoardElement[,] positions)
    {
        KeyValuePair<int, int> elem1Pos = BoardFunctions.GetPositionOfElement(elem1, positions);
        KeyValuePair<int, int> elem2Pos = BoardFunctions.GetPositionOfElement(elem2, positions);
        // Case same collumn, one row         above                                 or below
        if (elem1Pos.Key == elem2Pos.Key && (elem2Pos.Value == elem1Pos.Value - 1 || elem2Pos.Value == elem1Pos.Value + 1))
        {
            return true;
        }
        // Case same row, one collumn                left                                 or right
        else if (elem1Pos.Value == elem2Pos.Value && (elem2Pos.Key == elem1Pos.Key - 1 || elem2Pos.Key == elem1Pos.Key + 1))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    /// <summary> Returns if two elements are neighbours</summary>
    public static bool GetIfNeighbours(int elem1Collum, int elem1row, int elem2Collum, int elem2row, BoardElement[,] positions)
    {
        // Case same collumn, one row         above                                 or below
        if (elem1Collum == elem2Collum && (elem2row == elem1row - 1 || elem2row == elem1row + 1))
        {
            return true;
        }
        // Case same row, one collumn                left                                 or right
        else if (elem1row == elem2row && (elem2Collum == elem1Collum - 1 || elem2Collum == elem1Collum + 1))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    /// <summary> Get a color that does not exceeds max score allowed </summary>
    public static Color GetColorInScore(int collum, int row, ref BoardElement[,] positions, bool[,] matchedElemPositions, int totalCollums, int totalRows, int maxScoreAllowed, ref int currentScorePossible, bool forceGetMin)
    {
        int bestPossibleScoreFound = 0;
        int bestScoreIndex = UnityEngine.Random.Range(0, GetAvailableColors().Length);
        int minimumPossibleScoreFound = 10000000;
        int minimumScoreIndex = UnityEngine.Random.Range(0, GetAvailableColors().Length);
        int startingIndex = UnityEngine.Random.Range(0, GetAvailableColors().Length);

        for (int i = startingIndex; i < GetAvailableColors().Length + startingIndex; i++)
        {
            int index = i;
            if (index >= GetAvailableColors().Length)
            {
                index = i - GetAvailableColors().Length;
            }
            Color originalColor = positions[collum, row].GetElementValue();
            if (index >= GetAvailableColors().Length || index < 0)
            {
                Debug.LogError("Index out of range. StartingIndex: " + startingIndex + ", currentIndex: " + index);
            }
            positions[collum, row].SetValue(GetAvailableColors()[index]);

            int potentialScore = isPotentialInput(collum, row, positions, matchedElemPositions, totalCollums, totalRows);
            //Debug.Log("### Potential score: " + potentialScore);
            positions[collum, row].SetValue(originalColor);
            if (potentialScore <= maxScoreAllowed && potentialScore > bestPossibleScoreFound)
            {
                bestPossibleScoreFound = potentialScore;
                bestScoreIndex = index;
            }
            else if (minimumPossibleScoreFound > potentialScore)
            {
                minimumScoreIndex = index;
                minimumPossibleScoreFound = potentialScore;
            }
        }
        if (bestPossibleScoreFound > 0 && !forceGetMin)
        {
            Debug.Log("### SCORE ### Best score found: " + bestPossibleScoreFound + " for element " + positions[collum, row].GetAttachedGameObject().transform.parent.GetChild(bestScoreIndex));
            currentScorePossible = bestPossibleScoreFound;
            return GetAvailableColors()[bestScoreIndex];
        }
        else
        {
            Debug.Log("### SCORE ### Worst score found: " + minimumPossibleScoreFound + " for element " + positions[collum, row].GetAttachedGameObject().transform.parent.GetChild(bestScoreIndex));
            currentScorePossible = minimumPossibleScoreFound;
            return GetAvailableColors()[minimumScoreIndex];
        }
    }

    public static Color[] GetAvailableColors()
    {
        return availColors;
    }
    /// <summary> Highlights a cell by enabling the panel of it's child. Used to visualize correct input </summary>
    public static void ToggleHighlightCell(int collum, int row, BoardElement[,] positions, bool shouldHighlight, Color highlightColor)
    {
        if (shouldHighlight)
        {
            positions[collum, row].GetAttachedGameObject().transform.GetChild(0).gameObject.GetComponent<UnityEngine.UI.Image>().color = highlightColor;
            positions[collum, row].GetAttachedGameObject().transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            positions[collum, row].GetAttachedGameObject().transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    public static BoardElement GetElementBasedOnParentIndex(BoardElement[,] positions, int index)
    {
        for (int row = 0; row < positions.GetLength(1); row++)
        {
            for (int collum = 0; collum < positions.GetLength(0); collum++)
            {
                if (positions[collum, row].GetChildIndex() == index)
                {
                    return positions[collum, row];
                }
            }
        }
        Debug.LogError("Could not find element with parent index: " + index);
        return null;
    }

    #region Debug
    public static void CheckIfElementAtPositionIsNull(int collum, int row, BoardElement[,] positions)
    {
#if UNITY_EDITOR
        if (positions[collum, row] == null)
        {
            Debug.LogError("Position: (" + collum + ", " + row + "), is null");
        }
#endif
    }
    #endregion
}