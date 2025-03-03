using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace GCrazyGames
{
    /// <summary>
    /// Main game worker
    /// </summary>
    internal class GMain : MonoBehaviour
    {
        #region Mono fields

        /// <summary>
        /// The game panel
        /// </summary>
        public GameObject Game;
        /// <summary>
        /// The menu panel
        /// </summary>
        public GameObject Menu;
        /// <summary>
        /// Map container for the prefabs
        /// </summary>
        public Transform Map;
        /// <summary>
        /// The enemy icon
        /// </summary>
        public GameObject Enemy;
        /// <summary>
        /// The player icon
        /// </summary>
        public GameObject Player;
        /// <summary>
        /// The end-of-game panel
        /// </summary>
        public GameObject Result;
        /// <summary>
        /// Player points value object
        /// </summary>
        public TMP_Text PlayerValue;
        /// <summary>
        /// Enemy points value object
        /// </summary>
        public TMP_Text EnemyValue;
        /// <summary>
        /// Player win text
        /// </summary>
        public TMP_Text PlayerWin;
        /// <summary>
        /// Enemy win text
        /// </summary>
        public TMP_Text EnemyWin;
        /// <summary>
        /// Draw win text
        /// </summary>
        public TMP_Text FriendlyWin;
        /// <summary>
        /// Localizations
        /// </summary>
        public LocalizationSettings Localization;

        #endregion

        #region Variables

        /// <summary>
        /// Cube array of points
        /// </summary>
        private GPoint[,] FPoints;
        /// <summary>
        /// Start point of the drag
        /// </summary>
        private GPoint ActivePoint;
        /// <summary>
        /// Active point under the drag
        /// </summary>
        private GPoint TargetPoint;
        /// <summary>
        /// Turn owner state
        /// </summary>
        private GOwner FTurn;
        /// <summary>
        /// Physic map size
        /// </summary>
        private int FMapSize;
        /// <summary>
        /// Logic map size as game level
        /// </summary>
        private int FLevel;
        /// <summary>
        /// Player point count
        /// </summary>
        private int FPlayerValue;
        /// <summary>
        /// Enemy point count
        /// </summary>
        private int FEnemyValue;
        /// <summary>
        /// AI coroutine
        /// </summary>
        private Coroutine FCoroutineAI = null;
        /// <summary>
        /// The game stub name
        /// </summary>
        private const string csGameName = "point_lines";

        #endregion

        #region Methods

        /// <summary>
        /// Mono scene start
        /// </summary>
        public async void Start()
        {
            await GAnalytics.Init(csGameName);
            NavigationToMenu();
        }

        /// <summary>
        /// Mono scene end
        /// </summary>
        public void OnDestroy()
        {
            GAnalytics.Done();
        }

        /// <summary>
        /// Starting the new a game with selected level
        /// </summary>
        public void StartGame(int aLevel)
        {
            // Disable UI and set camera for selected game size
            Result.SetActive(false);
            Menu.SetActive(false);
            Game.SetActive(true);
            Camera.main.transform.position = new Vector3(3 + aLevel - 1, 3 + aLevel - 1, Camera.main.transform.position.z);
            // Change cubes count
            var tmpSize = aLevel switch
            {
                1 => 4,
                2 => 5,
                _ => 6,
            };
            FLevel = aLevel;
            FMapSize = tmpSize - 1;
            // Create the cube-point prefabs
            FPoints = new GPoint[tmpSize, tmpSize];
            for (int tmpX = 0; tmpX < tmpSize; tmpX++)
            {
                for (int tmpY = 0; tmpY < tmpSize; tmpY++)
                {
                    var tmpPoint = GUtils.CreatePrefab<GPoint>("Point", tmpX * 2, tmpY * 2, Map);
                    tmpPoint.Init(tmpX, tmpY);
                    tmpPoint.OnTouchBeginDrag += DoPointBeginDrag;
                    tmpPoint.OnTouchDrag += DoPointDrag;
                    tmpPoint.OnTouchEndDrag += DoPointEndDrag;
                    tmpPoint.OnTouchOverEnter += DoPointCheckEnter;
                    tmpPoint.OnTouchOverLeave += DoPointCheckLeave;
                    FPoints[tmpX, tmpY] = tmpPoint;
                }
            }
            // First step always by user
            SetTurn(GOwner.Main);
        }

        /// <summary>
        /// UI go to Menu
        /// </summary>
        public void UIGoToMenu()
        {
            NavigationToMenu();
            GAnalytics.GoToMenu();
        }

        /// <summary>
        /// UI go to Restart
        /// </summary>
        public void UIGoToRestart()
        {
            Clean();
            StartGame(FLevel);
            GAnalytics.RestartGame(FLevel);
        }

        /// <summary>
        /// UI selecting lvl of the game
        /// </summary>
        public void UIGoToLvl(int aLevel)
        {
            FLevel = aLevel;
            StartGame(aLevel);
            GAnalytics.StartGame(aLevel, Localization.GetSelectedLocale().name);
        }

        /// <summary>
        /// Select the next localization
        /// </summary>
        public void UINextLanguage()
        {
            var tmpLocale = Localization.GetSelectedLocale();
            var tmpLocales = Localization.GetAvailableLocales().Locales;
            var tmpIndex = tmpLocales.IndexOf(tmpLocale);
            if (tmpIndex < tmpLocales.Count - 1)
                Localization.SetSelectedLocale(tmpLocales[tmpIndex + 1]);
            else
                Localization.SetSelectedLocale(tmpLocales[0]);
        }

        /// <summary>
        /// Callback for start dragging
        /// </summary>
        /// <param name="aPoint">Drag object</param>
        private void DoPointBeginDrag(GTouchObject aPoint)
        {
            var tmpPoint = (GPoint)aPoint;
            // Cube-to-cube selection
            if (tmpPoint.Line.enabled)
                return;
            else
                tmpPoint.Line.enabled = true;
        }

        /// <summary>
        /// Callback for dragging
        /// </summary>
        /// <param name="aPoint">Drag object</param>
        private void DoPointDrag(GTouchObject aPoint)
        {
            var tmpPoint = (GPoint)aPoint;
            // Accept the target cube
            if (tmpPoint.Owner != GOwner.Enemy)
            {
                ActivePoint = tmpPoint;
                tmpPoint.MarkSelected(true);
                SelectTargetPoints(true);
            }
        }

        /// <summary>
        /// Callback for end of dragging
        /// </summary>
        /// <param name="aPoint">Object under drag</param>
        private void DoPointEndDrag(GTouchObject aPoint)
        {
            var tmpPoint = (GPoint)aPoint;
            // Create a new the wall
            if (TargetPoint != null && TargetPoint != ActivePoint)
                CreateWall(ActivePoint, TargetPoint);
            // Disable current line
            tmpPoint.Line.enabled = false;
            tmpPoint.MarkTarget(false);
            // Disable selecting for other cubes
            SelectTargetPoints(false);
            ActivePoint = null;
            TargetPoint = null;
        }

        /// <summary>
        /// Checker for point-to-point actions
        /// </summary>
        /// <param name="aPoint">Current point under the drag</param>
        private void DoPointCheckEnter(GTouchObject aPoint)
        {
            var tmpPoint = (GPoint)aPoint;
            // Check current values
            if (ActivePoint == null)
                return;
            if (ActivePoint == tmpPoint)
                return;
            if (ActivePoint.Links.Contains(tmpPoint))
                return;
            // Can select x/y similar coords
            if (tmpPoint.X == ActivePoint.X && (tmpPoint.Y == ActivePoint.Y - 1 || tmpPoint.Y == ActivePoint.Y + 1)
                || tmpPoint.Y == ActivePoint.Y && (tmpPoint.X == ActivePoint.X - 1 || tmpPoint.X == ActivePoint.X + 1))
            {
                tmpPoint.MarkSelected(true);
                TargetPoint = tmpPoint;
            }
        }

        /// <summary>
        /// Unselecting active points
        /// </summary>
        /// <param name="aPoint">Current point under the drag</param>
        private void DoPointCheckLeave(GTouchObject aPoint)
        {
            var tmpPoint = (GPoint)aPoint;
            // Check current values
            if (ActivePoint != tmpPoint && ActivePoint != null)
            {
                TargetPoint = null;
                tmpPoint.MarkSelected(false);
            }
        }

        /// <summary>
        /// Ingame menu renderer
        /// </summary>
        private void NavigationToMenu()
        {
            Clean();
            Result.SetActive(false);
            Game.SetActive(false);
            Menu.SetActive(true);
        }

        /// <summary>
        /// Clean map and return cntrol to user
        /// </summary>
        private void Clean()
        {
            if (FCoroutineAI != null)
                StopCoroutine(FCoroutineAI);
            for (int tmpIndex = 0; tmpIndex < Map.childCount; tmpIndex++)
                Destroy(Map.GetChild(tmpIndex).gameObject);
            SetPlayerValue(0);
            SetEnemyValue(0);
            SetTurn(GOwner.Main);
        }

        /// <summary>
        /// Show the game result, disable cubes
        /// </summary>
        private void ShowResult()
        {
            // Disable all
            FriendlyWin.gameObject.SetActive(false);
            EnemyWin.gameObject.SetActive(false);
            PlayerWin.gameObject.SetActive(false);
            // Enable actually
            if (FEnemyValue > FPlayerValue)
            {
                EnemyWin.gameObject.SetActive(true);
                GAnalytics.ResultLoose(FLevel);
            }
            else if (FEnemyValue < FPlayerValue)
            {
                PlayerWin.gameObject.SetActive(true);
                GAnalytics.ResultWin(FLevel);
            }
            else
            {
                FriendlyWin.gameObject.SetActive(true);
                GAnalytics.ResultDraw(FLevel);
            }
            // Disable intercative controls for the cubes
            SetTurn(GOwner.Enemy);
            Result.SetActive(true);
        }

        /// <summary>
        /// Setter for the player scoring
        /// </summary>
        /// <param name="aValue">Points count</param>
        private void SetPlayerValue(int aValue)
        {
            FPlayerValue = aValue;
            PlayerValue.text = aValue.ToString();
        }

        /// <summary>
        /// Setter for the enemy scoring
        /// </summary>
        /// <param name="aValue">Points count</param>
        private void SetEnemyValue(int aValue)
        {
            FEnemyValue = aValue;
            EnemyValue.text = aValue.ToString();
        }

        /// <summary>
        /// Change turn owner
        /// </summary>
        /// <param name="aOwner">Next owner</param>
        private void SetTurn(GOwner aOwner)
        {
            // Game is ended, animated is active
            if (Result.activeSelf)
                return;
            // Disable icons
            FTurn = aOwner;
            Enemy.SetActive(aOwner != GOwner.Main);
            Player.SetActive(aOwner == GOwner.Main);
            // Disable the points
            if (FPoints != null)
            {
                foreach (var tmpPoint in FPoints)
                    tmpPoint.SetEnable(FTurn == GOwner.Main);
            }
        }

        /// <summary>
        /// Calculate step for AI by X
        /// </summary>
        /// <param name="aSource">Cube from</param>
        /// <param name="aTarget">Cube to</param>
        /// <returns>Successful takeover</returns>
        private bool AiStepX(GPoint aSource, GPoint aTarget)
        {
            if (!aSource.Links.Contains(aTarget))
            {
                FindEdgeY(aSource.Links, aSource.Y, out GPoint tmpLeft, out GPoint tmpRight);
                FindEdgeY(aTarget.Links, aSource.Y, out GPoint tmpCounterLeft, out GPoint tmpCounterRight);
                bool tmpL = FindTerraY(aSource, aTarget, tmpLeft, tmpCounterLeft, -1);
                bool tmpR = FindTerraY(aSource, aTarget, tmpRight, tmpCounterRight, +1);
                if (tmpL || tmpR)
                {
                    CreateWallPrefab(aSource, aTarget);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Calculate step for AI by Y
        /// </summary>
        /// <param name="aSource">Cube from</param>
        /// <param name="aTarget">Cube to</param>
        /// <returns>Successful takeover</returns>
        private bool AiStepY(GPoint aSource, GPoint aTarget)
        {
            if (!aSource.Links.Contains(aTarget))
            {
                FindEdgeX(aSource.Links, aSource.X, out GPoint tmpLeft, out GPoint tmpRight);
                FindEdgeX(aTarget.Links, aSource.X, out GPoint tmpCounterLeft, out GPoint tmpCounterRight);
                bool tmpL = FindTerraX(aSource, aTarget, tmpLeft, tmpCounterLeft, -1);
                bool tmpR = FindTerraX(aSource, aTarget, tmpRight, tmpCounterRight, +1);
                if (tmpL || tmpR)
                {
                    CreateWallPrefab(aSource, aTarget);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Hardcoded step for AI
        /// </summary>
        /// <returns></returns>
        private IEnumerator AiStep()
        {
            // check all points
            bool tmpRestart;
            do
            {
                tmpRestart = false;
                foreach (var tmpPoint in FPoints)
                {
                    if (tmpPoint.X > 0)
                        tmpRestart |= AiStepX(FPoints[tmpPoint.X - 1, tmpPoint.Y], tmpPoint);
                    if (tmpPoint.X < FMapSize)
                        tmpRestart |= AiStepX(FPoints[tmpPoint.X + 1, tmpPoint.Y], tmpPoint);
                    if (tmpPoint.Y > 0)
                        tmpRestart |= AiStepY(FPoints[tmpPoint.X, tmpPoint.Y - 1], tmpPoint);
                    if (tmpPoint.Y < FMapSize)
                        tmpRestart |= AiStepY(FPoints[tmpPoint.X, tmpPoint.Y + 1], tmpPoint);
                    if (tmpRestart)
                    {
                        yield return new WaitForSeconds(1);
                        break;
                    }
                }
            } while (tmpRestart);

            // Make new the wall
            List<GPoint> tmpList = FPoints.Cast<GPoint>().ToList().OrderBy(aItem => Random.value).ToList();
            foreach (var tmpPoint in tmpList)
            {
                if (tmpPoint.X > 0 && !FPoints[tmpPoint.X - 1, tmpPoint.Y].Links.Contains(tmpPoint))
                {
                    CreateWallPrefab(FPoints[tmpPoint.X - 1, tmpPoint.Y], tmpPoint);
                    break;
                }
                else if (tmpPoint.X < FMapSize && !FPoints[tmpPoint.X + 1, tmpPoint.Y].Links.Contains(tmpPoint))
                {
                    CreateWallPrefab(FPoints[tmpPoint.X + 1, tmpPoint.Y], tmpPoint);
                    break;
                }
                else if (tmpPoint.Y > 0 && !FPoints[tmpPoint.X, tmpPoint.Y - 1].Links.Contains(tmpPoint))
                {
                    CreateWallPrefab(FPoints[tmpPoint.X, tmpPoint.Y - 1], tmpPoint);
                    break;
                }
                else if (tmpPoint.Y < FMapSize && !FPoints[tmpPoint.X, tmpPoint.Y + 1].Links.Contains(tmpPoint))
                {
                    CreateWallPrefab(FPoints[tmpPoint.X, tmpPoint.Y + 1], tmpPoint);
                    break;
                }
            }

            // UI timeout
            yield return new WaitForSeconds(1);
            // Return turn to main
            SetTurn(GOwner.Main);
            // Close the coroutine
            yield return null;
        }

        /// <summary>
        /// Find the edge of cube by X
        /// </summary>
        /// <param name="aLinks">Cube links</param>
        /// <param name="aX">Z from</param>
        /// <param name="aLeft">Left cube</param>
        /// <param name="aRight">Right cube</param>
        private void FindEdgeX(List<GPoint> aLinks, int aX, out GPoint aLeft, out GPoint aRight)
        {
            aLeft = null;
            aRight = null;
            foreach (var tmpPoint in aLinks)
            {
                if (tmpPoint.X == aX - 1)
                    aLeft = tmpPoint;
                if (tmpPoint.X == aX + 1)
                    aRight = tmpPoint;
            };
        }

        /// <summary>
        /// Find the edge of cube by Y
        /// </summary>
        /// <param name="aLinks">Cube links</param>
        /// <param name="aX">Z from</param>
        /// <param name="aLeft">Left cube (top)</param>
        /// <param name="aRight">Right cube (bottom)</param>
        private void FindEdgeY(List<GPoint> aLinks, int aY, out GPoint aLeft, out GPoint aRight)
        {
            aLeft = null;
            aRight = null;
            foreach (var tmpPoint in aLinks)
            {
                if (tmpPoint.Y == aY - 1)
                    aLeft = tmpPoint;
                if (tmpPoint.Y == aY + 1)
                    aRight = tmpPoint;
            };
        }

        /// <summary>
        /// Territory capture check by X
        /// </summary>
        /// <param name="aSource">From</param>
        /// <param name="aTarget">To</param>
        /// <param name="aLeft">1-st corner</param>
        /// <param name="aRight">2-nd corner</param>
        /// <param name="aOffset">Line offset</param>
        /// <returns>Capture success</returns>
        private bool FindTerraX(GPoint aSource, GPoint aTarget, GPoint aLeft, GPoint aRight, int aOffset)
        {
            if (aLeft != null && aRight != null && aLeft.Links.Contains(aRight))
            {
                var tmpX = aSource.X * 2 + aOffset;
                var tmpY = aSource.Y < aTarget.Y ? aSource.Y * 2 + 1 : aSource.Y * 2 - 1;
                return CreateTerraPrefab(tmpX, tmpY);
            }
            return false;
        }

        /// <summary>
        /// Territory capture check by Y
        /// </summary>
        /// <param name="aSource">From</param>
        /// <param name="aTarget">To</param>
        /// <param name="aLeft">1-st corner</param>
        /// <param name="aRight">2-nd corner</param>
        /// <param name="aOffset">Line offset</param>
        /// <returns>Capture success</returns>
        private bool FindTerraY(GPoint aSource, GPoint aTarget, GPoint aLeft, GPoint aRight, int aOffset)
        {
            if (aLeft != null && aRight != null && aLeft.Links.Contains(aRight))
            {
                var tmpX = aSource.X < aTarget.X ? aSource.X * 2 + 1 : aSource.X * 2 - 1;
                var tmpY = aSource.Y * 2 + aOffset;
                return CreateTerraPrefab(tmpX, tmpY);
            }
            return false;
        }

        /// <summary>
        /// Territory prefab creator
        /// </summary>
        /// <param name="aX">Prefab coord at X</param>
        /// <param name="aY">Prefab coord at Y</param>
        /// <returns>Function stub</returns>
        private bool CreateTerraPrefab(int aX, int aY)
        {
            GUtils.CreatePrefab<GTerra>("Terra", aX, aY, Map).SetOwner(FTurn);
            // Calc points
            if (FTurn == GOwner.Main)
                SetPlayerValue(FPlayerValue + 1);
            else
                SetEnemyValue(FEnemyValue + 1);
            // Calc end the game
            if (Mathf.Sqrt(FPlayerValue + FEnemyValue) + 1 == FMapSize + 1)
            {
                SetTurn(GOwner.Enemy);
                ShowResult();
            }
            // Next step approoved
            return true;
        }

        /// <summary>
        /// The wall prefab creator
        /// </summary>
        /// <param name="aSource">Left cube</param>
        /// <param name="aTarget">Right cube</param>
        private void CreateWallPrefab(GPoint aSource, GPoint aTarget)
        {
            aSource.Links.Add(aTarget);
            aTarget.Links.Add(aSource);

            var tmpX = aSource.X < aTarget.X ? aSource.X * 2 + 1 : aSource.X > aTarget.X ? aSource.X * 2 - 1 : aSource.X * 2;
            var tmpY = aSource.Y < aTarget.Y ? aSource.Y * 2 + 1 : aSource.Y > aTarget.Y ? aSource.Y * 2 - 1 : aSource.Y * 2;
            var tmpWall = GUtils.CreatePrefab<GWall>("Wall", tmpX, tmpY, Map);
            tmpWall.SetOwner(FTurn);
        }

        /// <summary>
        /// Wall automatic checker
        /// </summary>
        /// <param name="aSource"></param>
        /// <param name="aTarget"></param>
        private void CreateWall(GPoint aSource, GPoint aTarget)
        {
            // Adding new wall to scene
            CreateWallPrefab(aSource, aTarget);
            // .... ty c# for this
            bool tmpL;
            bool tmpR;
            // Check walls for x/y by line between source-target cubes
            if (aSource.Y == aTarget.Y)
            {
                FindEdgeY(aSource.Links, aSource.Y, out GPoint tmpLeft, out GPoint tmpRight);
                FindEdgeY(aTarget.Links, aSource.Y, out GPoint tmpCounterLeft, out GPoint tmpCounterRight);
                tmpL = FindTerraY(aSource, aTarget, tmpLeft, tmpCounterLeft, -1);
                tmpR = FindTerraY(aSource, aTarget, tmpRight, tmpCounterRight, +1);
            }
            else
            {
                FindEdgeX(aSource.Links, aSource.X, out GPoint tmpLeft, out GPoint tmpRight);
                FindEdgeX(aTarget.Links, aSource.X, out GPoint tmpCounterLeft, out GPoint tmpCounterRight);
                tmpL = FindTerraX(aSource, aTarget, tmpLeft, tmpCounterLeft, -1);
                tmpR = FindTerraX(aSource, aTarget, tmpRight, tmpCounterRight, +1);
            }
            // No the territory found, change turn owner
            if (!tmpL && !tmpR)
            {
                SetTurn(GOwner.Enemy);
                FCoroutineAI = StartCoroutine(AiStep());
            }
        }

        /// <summary>
        /// Changer for the cubes states
        /// </summary>
        /// <param name="aIsTargeted">Is targeted property</param>
        private void SelectTargetPoints(bool aIsTargeted)
        {
            for (var tmpX = ActivePoint.X - 1; tmpX <= ActivePoint.X + 1; tmpX++)
                if (tmpX >= 0 && tmpX <= FMapSize && tmpX != ActivePoint.X)
                    if (!aIsTargeted || !ActivePoint.Links.Contains(FPoints[tmpX, ActivePoint.Y]))
                        FPoints[tmpX, ActivePoint.Y].MarkTarget(aIsTargeted);

            for (var tmpY = ActivePoint.Y - 1; tmpY <= ActivePoint.Y + 1; tmpY++)
                if (tmpY >= 0 && tmpY <= FMapSize && tmpY != ActivePoint.Y)
                    if (!aIsTargeted || !ActivePoint.Links.Contains(FPoints[ActivePoint.X, tmpY]))
                        FPoints[ActivePoint.X, tmpY].MarkTarget(aIsTargeted);
        }

        #endregion
    }
}