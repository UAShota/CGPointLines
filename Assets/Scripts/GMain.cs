using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Services.Analytics;
using Unity.Services.Core;
using Unity.Services.Core.Analytics;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace GCrazyGames
{
    /// <summary>
    /// Main game worker
    /// </summary>
    public class GMain : MonoBehaviour
    {
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
        /// Mono scene start
        /// </summary>
        public async void Start()
        {
            Debug.Log("1");
            await UnityServices.InitializeAsync();
            AnalyticsService.Instance.StartDataCollection();
            Debug.Log("2");
            GoToMenu();
        }

        /// <summary>
        /// Callback for start dragging
        /// </summary>
        /// <param name="aPoint">Drag object</param>
        public void BeginDrag(GPoint aPoint)
        {
            // Cube-to-cube selection
            if (aPoint.Line.enabled)
                return;
            else
                aPoint.Line.enabled = true;
            // Accept the target cube
            if (aPoint.Owner != GOwner.Enemy)
            {
                ActivePoint = aPoint;
                aPoint.MarkSelected(true);
                SelectTargetPoints(true);
            }
        }

        /// <summary>
        /// Callback for end of dragging
        /// </summary>
        /// <param name="aPoint">Object under drag</param>
        public void EndDrag(GPoint aPoint)
        {
            if (TargetPoint != null && TargetPoint != ActivePoint)
                CreateWall(ActivePoint, TargetPoint);
            // Disable current line
            aPoint.Line.enabled = false;
            aPoint.MarkTarget(false);
            // Disable selecting for other cubes
            SelectTargetPoints(false);
            ActivePoint = null;
            TargetPoint = null;
        }

        /// <summary>
        /// Checker for point-to-point actions
        /// </summary>
        /// <param name="aPoint">Current point under the drag</param>
        public void CheckEnter(GPoint aPoint)
        {
            if (ActivePoint == null)
                return;
            if (ActivePoint == aPoint)
                return;
            if (ActivePoint.Links.Contains(aPoint))
                return;
            // Can select x/y similar coords
            if (aPoint.X == ActivePoint.X && (aPoint.Y == ActivePoint.Y - 1 || aPoint.Y == ActivePoint.Y + 1)
                || aPoint.Y == ActivePoint.Y && (aPoint.X == ActivePoint.X - 1 || aPoint.X == ActivePoint.X + 1))
            {
                aPoint.MarkSelected(true);
                TargetPoint = aPoint;
            }
        }

        /// <summary>
        /// Unselecting active points
        /// </summary>
        /// <param name="aPoint">Current point under the drag</param>
        public void CheckExit(GPoint aPoint)
        {
            if (ActivePoint != aPoint && ActivePoint != null)
            {
                TargetPoint = null;
                aPoint.MarkSelected(false);
            }
        }

        /// <summary>
        /// UI go to Menu
        /// </summary>
        public void GoToMenu()
        {
            Clean();
            Result.SetActive(false);
            Game.SetActive(false);
            Menu.SetActive(true);
        }

        /// <summary>
        /// UI go to Restart
        /// </summary>
        public void GoToRestart()
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "levelName", "level1"}
            };

            // The ‘levelCompleted’ event will get cached locally
            //and sent during the next scheduled upload, within 1 minute
            AnalyticsService.Instance.RecordEvent("leveluuuuuuuuuuuuu");


            Clean();
            StartGame(FLevel);
        }

        /// <summary>
        /// UI select lvl1 the game
        /// </summary>
        public void GoToLvl1()
        {
            StartGame(1);
        }

        /// <summary>
        /// UI select lvl2 the game
        /// </summary>
        public void GoToLvl2()
        {
            StartGame(2);
        }

        /// <summary>
        /// UI select lvl3 the game
        /// </summary>
        public void GoToLvl3()
        {
            StartGame(3);
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
                    FPoints[tmpX, tmpY] = GUtils.CreatePrefab<GPoint>("Point", tmpX * 2, tmpY * 2, Map);
                    FPoints[tmpX, tmpY].Init(this, tmpX, tmpY);
                }
            }
            // First step always by user
            SetTurn(GOwner.Main);
        }

        /// <summary>
        /// Select the next localization
        /// </summary>
        public void NextLanguage()
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
        /// Clean map and return cntrol to user
        /// </summary>
        private void Clean()
        {
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
                EnemyWin.gameObject.SetActive(true);
            else if (FEnemyValue < FPlayerValue)
                PlayerWin.gameObject.SetActive(true);
            else
                FriendlyWin.gameObject.SetActive(true);
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
                StartCoroutine(AiStep());
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
    }
}