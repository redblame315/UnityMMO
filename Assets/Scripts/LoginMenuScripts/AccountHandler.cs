﻿using UnityEngine;
using UnityEngine.UI;
using MMOServer;
using System;

public class AccountHandler : MonoBehaviour {
    public StatusBoxHandler statusBoxHandler;
    private PacketProcessor packetProcessor;

    //split this up into two methods, one for registering and one for logging in
    public void SubmitAccount(string findUser, string findPass, MenuLink ml, bool registering)
    {
        GameObject passwordGameObj = GameObject.Find(findPass);
        InputField passwordInput = passwordGameObj.GetComponent<InputField>();

        GameObject userGameObj = GameObject.Find(findUser);
        InputField usernameInput = userGameObj.GetComponent<InputField>();

        string password = passwordInput.text;
        string userName = usernameInput.text;

        //problem with this line as registering shouldn't go into CharacterMenu
        statusBoxHandler.InstantiatePrefab(Menus.CharacterMenu, MenuPrefabs.StatusBox);

        try
        {
            CheckInputs(userName, password);
            AccountPacket ap = new AccountPacket();
            byte[] data = ap.GetDataBytes(userName, password);


            SubPacket subPacket = new SubPacket(registering, (ushort)userName.Length, (ushort)password.Length, 0, 0, data, SubPacketTypes.Account);

            BasePacket packetToSend = BasePacket.CreatePacket(subPacket, false, false);
            Utils.SetAccountName(userName);
            packetProcessor = GameObject.FindGameObjectWithTag("PacketProcessor").GetComponent<PacketProcessor>();
            packetProcessor.LoginOrRegister(packetToSend);
        }
        catch (AccountException e)
        {
            StatusBoxHandler.statusText = e.Message;
            StatusBoxHandler.readyToClose = true;
        }






    }

    private void CheckInputs(string userName, string password)
    {
        if (password.Contains(" ") || userName.Contains(" "))
        {
            throw new AccountException("Invalid character in Username or Password");
        }
        if (password == null && userName == null)
        {
            throw new AccountException("Empty username or password");
        }
        if (password.Length < 4 || userName.Length < 3)
        {
            throw new AccountException("Password and Username length must be greater than 4 characters");
        }
    }
    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
public class AccountException : Exception
{
    public AccountException(string message) : base(message) { }
}