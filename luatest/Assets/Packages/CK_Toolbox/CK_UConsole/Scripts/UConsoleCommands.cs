namespace CardboardKeep
{
	using UnityEngine;
	using System.Collections;
	/// <summary>
	/// UConsole - A Valve-style in-game runtime command console for Unity games
	/// Author: Calum Spring
	/// E-mail: calum@cardboardkeep.com
	/// Creation Date: 7 November 2014
	/// Last Update: 26 April 2015
	/// License: You may modify these works and package them in a game release, commercial or otherwise, but you may not redistribute or resell this code, with or without modifications. This code is copyright Cardboard Keep PTY LTD and is protected by the Unity Asset Store commercial license (http://unity3d.com/legal/as_terms)
	/// Usage: Drag the Console prefab into your scene, press tilde (~) to activate, extend or add functions to the UConsoleCommands script to make them callable from the console. Requires Unity 4.6 or higher due to use of uGUI.
	/// </summary>
	public class UConsoleCommands : MonoBehaviour
	{
		protected UConsole console;

		protected void Start() { console = GetComponent<UConsole>(); }

		// Have game specific things you want to happen when the console is turned on and off? 
		// Add them to these functions or their equivalents in your extended script
		void GameSpecificActivate()
		{
			
		}
		void GameSpecificDeactivate()
		{
			
		}

		/// ====================================================================================
		/// Extend this class into a per-game custom commands script.
		/// Fill it with the functions you want to be able to call from the console.
		/// RULES
		/// 1. THESE FUNCTIONS MUST BE PUBLIC
		/// 2. FOR EASE OF TYPING IN-GAME THESE FUNCTIONS SHOULD BE ALL LOWERCASE
		/// 3. IF YOU WANT TO PASS ARGUMENTS: 
		/// 	3A. CREATE A PRIVATE FUNCTION WITH NO ARGUMENTS
		/// 	3B. CREATE A PUBLIC FUNCTION OF THE SAME NAME WITH NO BODY, GIVE IT YOUR ARGUMENTS
		/// 	3C. USE console.inputArgument IN THE PRIVATE FUNCTION TO UTILIZE THE INFORMATION
		/// 	3D. SEE argumentfunction BELOW AS AN EXAMPLE
		/// ====================================================================================

		public void testfunction()
		{
			UConsole.NewEvent("If you see this in the event log, testfunction was successfully called!");
		}

		private void argumentfunction()
		{
			UConsole.NewEvent("Your argument was: " + console.inputArgument);
		}
		public void argumentfunction(int number) { }

		public void mynewfunction()
		{
			UConsole.NewEvent("New Function was called.");
		}
	}
}