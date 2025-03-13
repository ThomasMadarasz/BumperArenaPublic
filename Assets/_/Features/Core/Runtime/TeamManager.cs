using Archi.Runtime;
using Enum.Runtime;
using Interfaces.Runtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.Runtime;

namespace Core.Runtime
{
    public class TeamManager : CNetBehaviour
    {
        #region Exposed

        public static TeamManager s_instance;

        #endregion


        #region Unity API

        private void Awake() => Setup();

        #endregion


        #region Main

        private void Setup()
        {
            s_instance = this;
        }

        public void SetupTeams(IEnumerable<ITeamable> players, TeamModeEnum teamMode)
        {
            List<ITeamable> teamables = players.ToList();

            foreach (ITeamable teamable in teamables)
            {
                teamable.SetPlayerID(teamables.FindIndex(a => a == teamable));
            }

            switch (teamMode)
            {
                case TeamModeEnum.Solo:

                    for (int i = 0; i < teamables.Count; i++)
                    {
                        teamables[i].SetTeamID(i + 1);
                    }

                    break;

                case TeamModeEnum.Duo:

                    //int firstplayer = Utility.m_rng.Next(0, teamables.Count);
                    //teamables[firstplayer].SetTeamID(1);

                    //int teamMateIndex = 0;

                    //do
                    //{
                    //    teamMateIndex = Utility.m_rng.Next(0, teamables.Count);
                    //} while (teamMateIndex != firstplayer);

                    //teamables[teamMateIndex].SetTeamID(1);

                    //for (int i = 0; i < teamables.Count; i++)
                    //{
                    //    if (i == teamMateIndex) continue;
                    //    teamables[i].SetTeamID(2);
                    //}

                    int teamId1 = 0;
                    int teamId2 = 0;

                    for (int i = 0; i < teamables.Count; i++)
                    {
                        if(teamId1 == 2) teamables[i].SetTeamID(2);
                        else if (teamId2 == 2) teamables[i].SetTeamID(1);
                        else
                        {
                            int teamId = Utility.m_rng.Next(1, 3);
                            if (teamId == 1) teamId1++;
                            else teamId2++;

                            teamables[i].SetTeamID(teamId);
                        }

                    }

                    break;

                default:
                    Debug.LogError("TeamMode not correct!");
                    break;
            }
        }

        #endregion
    }
}