using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Team : NetworkBehaviour
{
    #region Static Variables

    public static Transform teamsFolder;
    public static int nextTeamId = 0;
    public static List<Team> teams;

    #endregion

    #region Instance Variables

    public List<Spawn> spawns;

    public int teamId;

    TeamNetwork teamNetwork;

    #endregion

    #region MonoBehaviour Implementation



    #endregion

    #region NetworkBehaviour Implementation



    #endregion

    #region Public Static Methods

    public static Team NewTeam()
    {
        GameObject obj = Construction.main.InstantiateNewTeam();
        Team team = obj.GetComponent<Team>();

        team.GetComponent<NetworkObject>().Spawn();

        obj.transform.SetParent(teamsFolder);
        team.Setup(nextTeamId);

        teams.Add(team); // TODO: optimize: loop through list to find the place this fits instead of sorting after adding
        teams.Sort((gA, gB) => gB.teamId > gA.teamId ? -1 : 1);

        nextTeamId = 0;
        foreach (Team curTeam in teams)
        {
            if (curTeam.teamId > nextTeamId) break;
            nextTeamId++;
        }

        return team;
    }

    public static Team GetTeam(int teamId)
    {
        foreach (Team curTeam in teams)
        {
            if (curTeam.teamId == teamId) return curTeam;
        }

        return null;
    }

    public static void Initialize()
    {
        teamsFolder = GameObject.Find("Teams").transform;
        teams = new List<Team>();
    }

    #endregion

    #region Public Instance Methods

    public void AddSpawn(Spawn spawn)
    {
        spawns.Add(spawn);
        Debug.Log(teamNetwork);
        teamNetwork._spawns.Add(
            spawn.GetComponent<NetworkObject>()
        );
    }

    public void RemoveSpawn(Spawn spawn)
    {
        spawns.Remove(spawn);
        teamNetwork._spawns.Remove(spawn.GetComponent<NetworkObject>());
    }

    #endregion

    #region Private Methods

    private void Setup(int _teamId)
    {
        teamId = _teamId;
        teamNetwork = GetComponent<TeamNetwork>();
        spawns = new();
    }

    #endregion
}
