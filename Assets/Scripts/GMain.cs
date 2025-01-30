using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GCrazyGames
{
    public class GMain : MonoBehaviour
    {
        public GameObject Game;
        public GameObject Menu;
        public Transform Map;
        public GameObject Enemy;
        public GameObject Player;
        public GameObject GameOver;

        private GPoint[,] FPoints;
        private GPoint ActivePoint;
        private GPoint TargetPoint;
        private GOwner FTurn;
        private int FMapSize;
        private int FLevel;

        public void BeginDrag(GPoint aPoint)
        {
            if (aPoint.Line.enabled)
                return;
            else
                aPoint.Line.enabled = true;

            if (aPoint.Owner != GOwner.Enemy)
            {
                ActivePoint = aPoint;
                aPoint.MarkSelected(true);
                SelectTargetPoints(true);
            }
        }

        public void EndDrag(GPoint aPoint)
        {
            if (TargetPoint != null && TargetPoint != ActivePoint)
                CreateWall(ActivePoint, TargetPoint);

            aPoint.Line.enabled = false;
            aPoint.MarkTarget(false);

            SelectTargetPoints(false);
            ActivePoint = null;
            TargetPoint = null;
        }

        public void CheckEnter(GPoint aPoint)
        {
            if (ActivePoint == null)
                return;
            if (ActivePoint == aPoint)
                return;
            if (ActivePoint.Links.Contains(aPoint))
                return;
            if (aPoint.X == ActivePoint.X && (aPoint.Y == ActivePoint.Y - 1 || aPoint.Y == ActivePoint.Y + 1)
                || aPoint.Y == ActivePoint.Y && (aPoint.X == ActivePoint.X - 1 || aPoint.X == ActivePoint.X + 1))
            {
                aPoint.MarkSelected(true);
                TargetPoint = aPoint;
            }
        }

        public void CheckExit(GPoint aPoint)
        {
            if (ActivePoint != aPoint && ActivePoint != null)
            {
                TargetPoint = null;
                aPoint.MarkSelected(false);
            }
        }

        public void GoToMenu()
        {
            Clean();
            Game.SetActive(false);
            Menu.SetActive(true);
        }

        public void GoToRestart()
        {
            Clean();
            StartGame(FLevel);
        }

        public void GoToLvl1()
        {
            StartGame(1);
        }

        public void GoToLvl2()
        {
            StartGame(2);
        }

        public void GoToLvl3()
        {
            StartGame(3);
        }

        public void StartGame(int aLevel)
        {
            Menu.SetActive(false);
            Game.SetActive(true);
            Camera.main.transform.position = new Vector3(3 + aLevel - 1, 3 + aLevel - 1, Camera.main.transform.position.z);

            var tmpSize = aLevel switch
            {
                1 => 4,
                2 => 5,
                _ => 6,
            };
            FLevel = aLevel;
            FMapSize = tmpSize - 1;

            FPoints = new GPoint[tmpSize, tmpSize];
            for (int tmpX = 0; tmpX < tmpSize; tmpX++)
            {
                for (int tmpY = 0; tmpY < tmpSize; tmpY++)
                {
                    FPoints[tmpX, tmpY] = GUtils.CreatePrefab<GPoint>("Point", tmpX * 2, tmpY * 2, Map);
                    FPoints[tmpX, tmpY].Init(this, tmpX, tmpY);
                }
            }

            SetTurn(GOwner.Main);
        }

        private void Clean()
        {
            for (int tmpIndex = 0; tmpIndex < Map.childCount; tmpIndex++)
                Destroy(Map.GetChild(tmpIndex).gameObject);
        }

        private void SetTurn(GOwner aOwner)
        {
            FTurn = aOwner;
            foreach (var tmpPoint in FPoints)
                tmpPoint.SetEnable(FTurn == GOwner.Main);
            Enemy.SetActive(aOwner != GOwner.Main);
            Player.SetActive(aOwner == GOwner.Main);
        }

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

        private IEnumerator AiStep()
        {
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

            yield return new WaitForSeconds(1);

            SetTurn(GOwner.Main);

            yield return null;
        }

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

        private bool FindTerraX(GPoint aSource, GPoint aTarget, GPoint aLeft, GPoint aRight, int aOffset)
        {
            if (aLeft != null && aRight != null && aLeft.Links.Contains(aRight))
            {
                var tmpX = aSource.X * 2 + aOffset;
                var tmpY = aSource.Y < aTarget.Y ? aSource.Y * 2 + 1 : aSource.Y * 2 - 1;
                var tmpTerra = GUtils.CreatePrefab<GTerra>("Terra", tmpX, tmpY, Map);
                tmpTerra.SetOwner(FTurn);
                return true;
            }
            return false;
        }

        private bool FindTerraY(GPoint aSource, GPoint aTarget, GPoint aLeft, GPoint aRight, int aOffset)
        {
            if (aLeft != null && aRight != null && aLeft.Links.Contains(aRight))
            {
                var tmpX = aSource.X < aTarget.X ? aSource.X * 2 + 1 : aSource.X * 2 - 1;
                var tmpY = aSource.Y * 2 + aOffset;
                var tmpTerra = GUtils.CreatePrefab<GTerra>("Terra", tmpX, tmpY, Map);
                tmpTerra.SetOwner(FTurn);
                return true;
            }
            return false;
        }

        private void CreateWallPrefab(GPoint aSource, GPoint aTarget)
        {
            aSource.Links.Add(aTarget);
            aTarget.Links.Add(aSource);

            var tmpX = aSource.X < aTarget.X ? aSource.X * 2 + 1 : aSource.X > aTarget.X ? aSource.X * 2 - 1 : aSource.X * 2;
            var tmpY = aSource.Y < aTarget.Y ? aSource.Y * 2 + 1 : aSource.Y > aTarget.Y ? aSource.Y * 2 - 1 : aSource.Y * 2;
            var tmpWall = GUtils.CreatePrefab<GWall>("Wall", tmpX, tmpY, Map);
            tmpWall.SetOwner(FTurn);
        }

        private void CreateWall(GPoint aSource, GPoint aTarget)
        {
            CreateWallPrefab(aSource, aTarget);

            bool tmpL;
            bool tmpR;

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

            if (!tmpL && !tmpR)
            {
                SetTurn(GOwner.Enemy);
                StartCoroutine(AiStep());
            }
        }

        private void SelectTargetPoints(bool aIsTargeted)
        {
            for (var tmpX = ActivePoint.X - 1; tmpX <= ActivePoint.X + 1; tmpX++)
                if (tmpX >= 0 && tmpX < FMapSize && tmpX != ActivePoint.X)
                    if (!aIsTargeted || !ActivePoint.Links.Contains(FPoints[tmpX, ActivePoint.Y]))
                        FPoints[tmpX, ActivePoint.Y].MarkTarget(aIsTargeted);

            for (var tmpY = ActivePoint.Y - 1; tmpY <= ActivePoint.Y + 1; tmpY++)
                if (tmpY >= 0 && tmpY < FMapSize && tmpY != ActivePoint.Y)
                    if (!aIsTargeted || !ActivePoint.Links.Contains(FPoints[ActivePoint.X, tmpY]))
                        FPoints[ActivePoint.X, tmpY].MarkTarget(aIsTargeted);
        }
    }
}