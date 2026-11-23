using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] private GameObject escapePanel;

    public void LeftDown()
    {
        player.inputLeft = true;
    }

    public void LeftUp()
    {
        player.inputLeft = false;
    }

    public void RightDown()
    {
        player.inputRight = true;
    }

    public void RightUp()
    {
        player.inputRight = false;
    }

    public void JumpDown()
    {
        player.inputJump = true;
    }

    public void JumpUp()
    {
        player.inputJump = false;
    }

    public void Escape()
    {
        Application.Quit();
    }

    public void CancelQuit()
    {
        escapePanel.SetActive(false);
    }

}