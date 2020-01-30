using UnityEngine;
using System;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public class GameManager : MonoBehaviour
{
    public int m_NumRoundsToWin = 5;
    public int m_NumRoundsToLose = 3;
    public float m_StartDelay = 3f;         
    public float m_EndDelay = 3f;  
    public Text m_MessageText;              
    public GameObject m_TankPrefab;         
    public TankManager[] m_Tanks;
    public GameObject m_cam;
    private Vector3 m_cam_original_pos;
    private Quaternion m_cam_original_rot;

    private int m_RoundNumber;              
    private WaitForSeconds m_StartWait;     
    private WaitForSeconds m_EndWait;       
    private TankManager m_RoundWinner;
    private TankManager m_GameWinner;       


    private void Start()
    {
        
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);

        SpawnAllTanks();
        StartCoroutine(GameLoop());
    }


    private void SpawnAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].m_Instance =
                Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
            m_Tanks[i].m_PlayerNumber = i + 1;
            m_Tanks[i].Setup();
        }
    }

    private IEnumerator GameLoop()
    {
        m_cam_original_pos = m_cam.transform.position;
        m_cam_original_rot = m_cam.transform.rotation;
        yield return StartCoroutine(RoundStarting());
        yield return StartCoroutine(RoundPlaying());
        yield return StartCoroutine(RoundEnding());

        if (m_GameWinner != null || m_NumRoundsToLose-m_Tanks[0].m_Lose<=0)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            StartCoroutine(GameLoop());
        }
    }


    private IEnumerator RoundStarting()
    {
        ResetAllTanks();
        //cam.transform.SetParent(m_TankPrefab[0]);
        m_Tanks[0].InsertChild(m_cam);
        DisableTankControl();


        m_RoundNumber += 1;
        m_MessageText.text = "ROUND" + m_RoundNumber;
        yield return m_StartWait;
    }


    private IEnumerator RoundPlaying()
    {
        EnableTankControl();
        m_MessageText.text = string.Empty;

        while (!OneTankLeft())
        {
            yield return null;
        }
        yield return null;
    }


    private IEnumerator RoundEnding()
    {
        DisableTankControl();
        m_cam.transform.SetParent(null);
        m_cam.transform.position = m_cam_original_pos;
        m_cam.transform.rotation = m_cam_original_rot;
        m_RoundWinner = GetRoundWinner();
        if (m_RoundWinner != null)
        {
            if(m_RoundWinner == m_Tanks[0])
            {
                m_RoundWinner.m_Wins++;
            }
            else
            {
                m_Tanks[0].m_Lose++;
            }
        }
        m_GameWinner = GetGameWinner();

        string message = EndMessage();

        m_MessageText.text = message;
        yield return m_EndWait;
    }


    private bool OneTankLeft()
    {
        int numTanksLeft = 0;

        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf)
                numTanksLeft++;
        }

        //return numTanksLeft <= 1;
        return !m_Tanks[0].m_Instance.activeSelf || numTanksLeft<=1;
    }


    private TankManager GetRoundWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf)
                return m_Tanks[i];
        }

        return null;
    }


    private TankManager GetGameWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                return m_Tanks[i];
        }

        return null;
    }


    private string EndMessage()
    {
        string message = "DRAW!";

        if (m_RoundWinner == m_Tanks[0])
            message = m_Tanks[0].m_ColoredPlayerText + " WINS THE ROUND!";
        else
            message = m_Tanks[0].m_ColoredPlayerText + ":"+ (m_NumRoundsToLose-m_Tanks[0].m_Lose) + "LIVES";

        /*message += "\n\n\n\n";

        for (int i = 0; i < m_Tanks.Length; i++)
        {
            message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " WINS\n";
        }*/

        if (m_GameWinner != null)
            message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";
        else if (m_NumRoundsToLose-m_Tanks[0].m_Lose<=0)
        {
            message = m_Tanks[0].m_ColoredPlayerText + " LOSES THE GAME!";
        }

        return message;
    }


    private void ResetAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].Reset();
        }
    }


    private void EnableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].EnableControl();
        }
    }


    private void DisableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].DisableControl();
        }
    }
}