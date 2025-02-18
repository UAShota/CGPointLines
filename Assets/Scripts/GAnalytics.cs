using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;
using Event = Unity.Services.Analytics.Event;

namespace GCrazyGames
{
    /// <summary>
    /// Ingame analytics event
    /// </summary>
    internal class GAnalyticsEvent : Event
    {
        /// <summary>
        /// Similar event construction
        /// </summary>
        /// <param name="aName">Event name</param>
        internal GAnalyticsEvent(string aName) : base(aName)
        {
        }

        /// <summary>
        /// Overrided string setter
        /// </summary>
        /// <param name="aName">Param name</param>
        /// <param name="aValue">Param value</param>
        internal void SetString(string aName, string aValue)
            => SetParameter(aName, aValue);

        /// <summary>
        /// Overrided int setter
        /// </summary>
        /// <param name="aName">Param name</param>
        /// <param name="aValue">Param value</param>
        internal void SetInt(string aName, int aValue)
            => SetParameter(aName, aValue);
    }

    /// <summary>
    /// Ingame analytics controller
    /// </summary>
    internal static class GAnalytics
    {
        /// <summary>
        /// Default analytics event/param preffix
        /// </summary>
        private const string csPeffix = "cg_";
        /// <summary>
        /// The current game
        /// </summary>
        private static string FGameName;

        /// <summary>
        /// Async class initializer
        /// </summary>
        /// <param name="aGameName">Game name</param>
        /// <returns>Task event</returns>
        internal static async Task Init(string aGameName)
        {
            FGameName = aGameName;
            try
            {
                await UnityServices.InitializeAsync();
                AnalyticsService.Instance.StartDataCollection();
            }
            catch (Exception E)
            {
                Debug.LogErrorFormat("GA start failed, {0}", E.Message);
            }
        }

        /// <summary>
        /// Class finalizer
        /// </summary>
        internal static void Done()
        {
            try
            {
                AnalyticsService.Instance.StopDataCollection();
            }
            catch (Exception E)
            {
                Debug.LogErrorFormat("GA stop failed, {0}", E.Message);
            }
        }

        /// <summary>
        /// The event about the game is started
        /// </summary>
        /// <param name="aLevel">Game level</param>
        /// <param name="aLang">Selected language</param>
        internal static void StartGame(int aLevel, string aLang)
        {
            SendEvent("start_game", new Dictionary<string, object>()
            {
                ["level"] = aLevel,
                ["lang"] = aLang
            });
        }

        /// <summary>
        /// The event about the game is restarted
        /// </summary>
        /// <param name="aLevel">Game level</param>
        internal static void RestartGame(int aLevel)
        {
            SendEvent("restart_game", new Dictionary<string, object>()
            {
                ["level"] = aLevel
            });
        }

        /// <summary>
        /// The event about navigation to the menu
        /// </summary>
        internal static void GoToMenu()
        {
            SendEvent("menu_game");
        }

        /// <summary>
        /// The event about the game result as win
        /// </summary>
        /// <param name="aLevel">Game level</param>
        internal static void ResultWin(int aLevel)
        {
            SendEvent("game_result", new Dictionary<string, object>()
            {
                ["endgame"] = "win",
                ["level"] = aLevel
            });
        }

        /// <summary>
        /// The event about the game result as draw
        /// </summary>
        /// <param name="aLevel">Game level</param>
        internal static void ResultDraw(int aLevel)
        {
            SendEvent("game_result", new Dictionary<string, object>()
            {
                ["endgame"] = "draw",
                ["level"] = aLevel
            });
        }

        /// <summary>
        /// The event about the game result as loose
        /// </summary>
        /// <param name="aLevel">Game level</param>
        internal static void ResultLoose(int aLevel)
        {
            SendEvent("game_result", new Dictionary<string, object>()
            {
                ["endgame"] = "loose",
                ["level"] = aLevel
            });
        }

        /// <summary>
        /// Sending an event with parameters
        /// </summary>
        /// <param name="aName">Event name</param>
        /// <param name="aParams">Params dictionary</param>
        private static void SendEvent(string aName, Dictionary<string, object> aParams = null)
        {
            try
            {
                GAnalyticsEvent tmpEvent = new(csPeffix + aName);
                if (aParams == null)
                    aParams = new Dictionary<string, object>();
                // Static params
                aParams.Add("game_name", FGameName);
                // Adding other params
                foreach (var tmpParam in aParams)
                {
                    string tmpName = csPeffix + tmpParam.Key;
                    // Check data type
                    if (tmpParam.Value is int tmpInt)
                        tmpEvent.SetInt(tmpName, tmpInt);
                    else if (tmpParam.Value is string tmpStr)
                        tmpEvent.SetString(tmpName, tmpStr);
                }
                // Try to send
                AnalyticsService.Instance.RecordEvent(tmpEvent);
            }
            catch (Exception E)
            {
                Debug.LogErrorFormat("GA send failed, {0}", E.Message);
            }
        }
    }
}