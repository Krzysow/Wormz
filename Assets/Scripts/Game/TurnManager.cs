using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum EndGame { InProgress, PlayerTeamWins, EnemyTeamWins, Draw }

public class TurnManager : MonoBehaviour
{
    public Vector3 cameraOffset = new Vector3(0, 30, -10);

    List<Character> teamPlayerCharacters = new List<Character>();
    List<Character> teamEnemyCharacters = new List<Character>();
    Character currentCharacter;
    float timer = 0f;
    float turnTime = 5f;
    int playerIndex = 0;
    int enemyIndex = 0;
    bool teamPlayerTurn = true;
    EndGame result = EndGame.InProgress;

    // Start is called before the first frame update
    void Start()
    {
        timer = turnTime;
        foreach (Character character in FindObjectsOfType<Character>())
        {
            if (character.GetIsOnPlayerTeam())
                teamPlayerCharacters.Add(character);
            else
                teamEnemyCharacters.Add(character);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (currentCharacter)
            Camera.main.transform.position = currentCharacter.transform.position + cameraOffset;

        if (result == EndGame.InProgress)
        {
            bool areBulletsInFlight = FindObjectsOfType<Bullet>().Length > 0;
            if (timer >= turnTime && !areBulletsInFlight)
            {
                timer = 0f;

                //List<Character> toRemove = new List<Character>();
                //foreach (Character character in teamPlayerCharacters)
                //{
                //    if (character.GetHealth() <= 0)
                //        toRemove.Add(character);
                //}
                //foreach (Character remove in toRemove)
                //    teamPlayerCharacters.Remove(remove);

                //toRemove.Clear();
                //foreach (Character character in teamEnemyCharacters)
                //{
                //    if (character.GetHealth() <= 0)
                //        toRemove.Add(character);
                //}
                //foreach (Character remove in toRemove)
                //    teamEnemyCharacters.Remove(remove);

                if (teamEnemyCharacters.Count == 0)
                    result = EndGame.PlayerTeamWins;
                if (teamPlayerCharacters.Count == 0)
                    result |= EndGame.EnemyTeamWins;

                if (currentCharacter)
                    currentCharacter.SetHasTurn(false);

                if (teamPlayerCharacters.Count > 0 && teamEnemyCharacters.Count > 0)
                {
                    if (teamPlayerTurn)
                    {
                        playerIndex = (playerIndex + 1) % teamPlayerCharacters.Count;
                        currentCharacter = teamPlayerCharacters[playerIndex];
                        currentCharacter.SetHasTurn(true);
                    }
                    else if (!teamPlayerTurn)
                    {
                        enemyIndex = (enemyIndex + 1) % teamEnemyCharacters.Count;
                        currentCharacter = teamEnemyCharacters[enemyIndex];
                        currentCharacter.SetHasTurn(true);
                    }
                }

                teamPlayerTurn = !teamPlayerTurn;
            }

            timer += Time.deltaTime;
        }
        else
        {
            Debug.Log(result);
        }
    }

    public float GetTimer()
    {
        return timer;
    }

    public void SetTimer(float newTimer)
    {
        timer = newTimer;
    }

    public void RemoveTeammate(Character character)
    {
        teamPlayerCharacters.Remove(character);
        teamEnemyCharacters.Remove(character);
    }
}
