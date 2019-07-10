 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mazematics : MonoBehaviour {

	public Material circleMat, squareMat, triangleMat, needleMat, diamondMat, hexagonMat, starMat, heartMat, shapelessMat, testCard;
	
	private int circlePlus, squarePlus, trianglePlus, needlePlus, diamondPlus, hexagonPlus, starPlus, heartPlus;
	private int circleMinus, squareMinus, triangleMinus, needleMinus, diamondMinus, hexagonMinus, starMinus, heartMinus;
	private int leftover;

	private int startValue, currentValue, goalValue;

	private int startMazeX, startMazeY, currentMazeX, currentMazeY;

	List<int> row1 = new List<int>();
	List<int> row2 = new List<int>();
	List<int> row3 = new List<int>();
	List<int> row4 = new List<int>();
	List<int> row5 = new List<int>();
	List<int> row6 = new List<int>();
	List<int> row7 = new List<int>();
	List<int> row8 = new List<int>();

	List<List<int>> maze = new List<List<int>>();

	List<string> logCoordinates = new List<string>();

	List<int> bannedValues = new List<int>();
	private string bannedValuesLogName;

	List<Material> shapeList = new List<Material>();
	
	public KMSelectable arrowUp, arrowDown, arrowLeft, arrowRight;
	public KMSelectable screen;

	public KMBombInfo bombInfo;
	public KMBombModule bombModule;
	public KMAudio bombAudio;

	public GameObject screenTextObject;
	private TextMesh screenTextMesh;
	private MeshRenderer screenTextRenderer;

	public GameObject quadObject;
	private MeshRenderer quadRenderer;

	private bool movedOnce; //tracks if the player has made one move

	private int moduleStage;
	//stage 0 before activation; 1 before player clicks screen; 2 while player is solving; 3 is solved

	private static int _moduleIdCounter = 1;
	private int _moduleId;

	void Start () {

		_moduleId = _moduleIdCounter++;
		
		quadRenderer = quadObject.GetComponent<MeshRenderer>();
		screenTextRenderer = screenTextObject.GetComponent<MeshRenderer>();
		screenTextMesh = screenTextObject.GetComponent<TextMesh>();
		
		///GIVING VALUES TO SHAPES///

		/* This loop adds numbers 1-9 into a list with a for loop.
		Later, each of the 8 shapes randomly picks one of these numbers,
		and assigns it's negative counterpart. */

		List<int> values = new List<int>();
		
		for (int i = 1; i < 10; i++)
        {
        	values.Add(i);
		}

		/* This loop fills a list with a material that will never be
		seen in the game, it simply provides empty list entries that
		can be overridden by the correct shapes in the next block.*/

		for (int i = 0; i < 10; i++)
		{
			shapeList.Add(shapelessMat);
		}

		circlePlus = values[Random.Range(0, values.Count)];
		circleMinus = circlePlus * -1;
		shapeList[circlePlus] = circleMat;
		values.Remove(circlePlus);

		squarePlus = values[Random.Range(0, values.Count)];
		squareMinus = squarePlus * -1;
		shapeList[squarePlus] = squareMat;
		values.Remove(squarePlus);

		trianglePlus = values[Random.Range(0, values.Count)];
		triangleMinus = trianglePlus * -1;
		shapeList[trianglePlus] = triangleMat;
		values.Remove(trianglePlus);

		needlePlus = values[Random.Range(0, values.Count)];
		needleMinus = needlePlus * -1;
		shapeList[needlePlus] = needleMat;
		values.Remove(needlePlus);

		diamondPlus = values[Random.Range(0, values.Count)];
		diamondMinus = diamondPlus * -1;
		shapeList[diamondPlus] = diamondMat;
		values.Remove(diamondPlus);

		hexagonPlus = values[Random.Range(0, values.Count)];
		hexagonMinus = hexagonPlus * -1;
		shapeList[hexagonPlus] = hexagonMat;
		values.Remove(hexagonPlus);

		starPlus = values[Random.Range(0, values.Count)];
		starMinus = starPlus * -1;
		shapeList[starPlus] = starMat;
		values.Remove(starPlus);

		heartPlus = values[Random.Range(0, values.Count)];
		heartMinus = heartPlus * -1;
		shapeList[heartPlus] = heartMat;
		values.Remove(heartPlus);

		leftover = values[0];

		///MAKING THE MAZE///

		row1.AddRange(new[] {hexagonPlus, starPlus, needleMinus, squareMinus, circlePlus, diamondPlus, triangleMinus, heartMinus});
		row2.AddRange(new[] {triangleMinus, heartPlus, diamondMinus, circleMinus, needlePlus, squarePlus, hexagonMinus, starPlus});
		row3.AddRange(new[] {starPlus, triangleMinus, squarePlus, needlePlus, diamondMinus, circleMinus, heartPlus, hexagonMinus});
		row4.AddRange(new[] {heartPlus, hexagonMinus, circleMinus, diamondPlus, starMinus, needlePlus, squarePlus, triangleMinus});
		row5.AddRange(new[] {needleMinus, squarePlus, starPlus, heartMinus, trianglePlus, hexagonMinus, diamondMinus, circlePlus});
		row6.AddRange(new[] {diamondMinus, circlePlus, hexagonMinus, trianglePlus, heartMinus, starMinus, needlePlus, squarePlus});
		row7.AddRange(new[] {circlePlus, diamondMinus, heartPlus, hexagonPlus, squareMinus, trianglePlus, starMinus, needleMinus});
		row8.AddRange(new[] {squareMinus, needleMinus, trianglePlus, starMinus, hexagonPlus, heartMinus, circlePlus, diamondPlus});

		maze.AddRange(new [] {row1, row2, row3, row4, row5, row6, row7, row8});

		/* Maze is a list of lists, effectively forming a 2D grid.
		References to maze[y][x] later in the script are essentially
		accessing coordinates in this grid. Y comes before X, numbers
		of course starting with [0][0] at the top-left corner. */

		///KM SELECTABLES///

		arrowUp.OnInteract += () => Move(-1, 0);
		arrowDown.OnInteract += () => Move(1, 0);
		arrowLeft.OnInteract += () => Move(0, -1);
		arrowRight.OnInteract += () => Move(0, 1);

		screen.OnInteract += ScreenClick;
		bombModule.OnActivate += SetActive;

		//used for printing maze coordinates in 'A1' format to log
		logCoordinates.AddRange(new[] {"A", "B", "C", "D", "E", "F", "G", "H"});

		generatePuzzle();
	}

	private void generatePuzzle () {
			
		///STARTING COORDINATE AND VALUES///

		startMazeX = Random.Range(0, row1.Count);
		startMazeY = Random.Range(0, maze.Count);
		currentMazeX = startMazeX;
		currentMazeY = startMazeY;

		startValue = Random.Range(17, 33);
		currentValue = startValue + maze[startMazeY][startMazeX];

		///PICKING BANNED VALUE RULE///

		if (squarePlus == (Mathf.Abs(maze[startMazeY][startMazeX])) || (diamondPlus == Mathf.Abs(maze[startMazeY][startMazeX])))
		{
			bannedValues.AddRange(new [] {1, 3, 6, 10, 15, 21, 28, 36, 45, 55, 66, 78, 91});
			bannedValuesLogName = "Triangular Numbers";
		}
		else if (startValue % 3 == 0)
		{
			bannedValues.AddRange(new [] {0, 7, 14, 21, 28, 35, 42, 49, 56, 63, 70, 77, 84, 91, 98});
			bannedValuesLogName = "Multiples of 7";
		}
		else if (Mathf.Sign(maze[startMazeY][startMazeX]) == 1)
		{
			bannedValues.AddRange(new [] {2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97});
			bannedValuesLogName = "Primes";
		}
		else
		{
			bannedValues.AddRange(new [] {1, 2, 3, 5, 8, 13, 21, 34, 55, 89});
			bannedValuesLogName = "Fibonacci Sequence Values";
		}

		for (int i = 1; i < 5; i++)
		{
			int rndm = Random.Range(1, 5);
			
			if (rndm == 1)
			{
				currentMazeX += 1;
				currentMazeX = currentMazeX % 8;
			}
			else if (rndm == 2)
			{
				currentMazeX -= 1;
				currentMazeX = (currentMazeX + 8) % 8;
			}
			else if (rndm == 3)
			{
				currentMazeY += 1;
				currentMazeY = currentMazeY % 8;
			}
			else
			{
				currentMazeY -= 1;
				currentMazeY = (currentMazeY + 8) % 8;
			}

			currentValue += maze[currentMazeY][currentMazeX];

			if (currentValue < 0 || currentValue > 49)
			{
				bannedValues.Clear();
				generatePuzzle();

				break;
			}
			else if (((i > 1) && bannedValues.Contains(currentValue) == true) || (i == 4))
			{				
				goalValue = currentValue;

				currentValue = startValue;
				currentMazeX = startMazeX;
				currentMazeY = startMazeY;
				
				break;
			}
		}

	Logging();
	}

	private void Logging () {
		
		Debug.LogFormat("[Mazematics #{0}] Circle = {1}", _moduleId, circlePlus);
		Debug.LogFormat("[Mazematics #{0}] Square = {1}", _moduleId, squarePlus);
		Debug.LogFormat("[Mazematics #{0}] Triangle = {1}", _moduleId, trianglePlus);
		Debug.LogFormat("[Mazematics #{0}] Needle = {1}", _moduleId, needlePlus);
		Debug.LogFormat("[Mazematics #{0}] Diamond = {1}", _moduleId, diamondPlus);
		Debug.LogFormat("[Mazematics #{0}] Hexagon = {1}", _moduleId, hexagonPlus);
		Debug.LogFormat("[Mazematics #{0}] Star = {1}", _moduleId, starPlus);
		Debug.LogFormat("[Mazematics #{0}] Heart = {1}", _moduleId, heartPlus);

		Debug.LogFormat("[Mazematics #{0}] Restricted values are {1}", _moduleId, bannedValuesLogName);

		Debug.LogFormat("[Mazematics #{0}] Initial value: {1}", _moduleId, startValue);
		Debug.LogFormat("[Mazematics #{0}] Goal value: {1}", _moduleId, goalValue);

		Debug.LogFormat("[Mazematics #{0}] BEGIN", _moduleId);
	}
	
	private void SetActive () {
		
		moduleStage = 1;
	}

	bool ScreenClick () {
	
		if (moduleStage == 1)
		{
			currentValue += maze[currentMazeY][currentMazeX];
			moduleStage = 2;
		}
		else if (moduleStage == 2)
		{			
			if (movedOnce == true) //prevents mashing reset from spamming the log
			{
				Debug.LogFormat("[Mazematics #{0}] MODULE RESET", _moduleId);
			}

			currentValue = startValue;
			currentMazeX = startMazeX;
			currentMazeY = startMazeY;
			moduleStage = 1;
			movedOnce = false;
		}
		return false;
	}
	
	bool Move (int y, int x) {

		bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		
		if (moduleStage == 2)
		{
			currentMazeX += x;
			currentMazeY += y;

			//adding 8 before performing modulo because C# cannot correctly modulo negatives
			currentMazeX = (currentMazeX + 8) % 8; 
			currentMazeY = (currentMazeY + 8) % 8;

			currentValue += maze[currentMazeY][currentMazeX];

			if (movedOnce)
			{
				if (currentValue == goalValue)
				{
					Debug.LogFormat("[Mazematics #{0}] Moved to: {1}{2} (cell is equal to {3})", _moduleId, logCoordinates[currentMazeX], (startMazeY + 1), maze[currentMazeY][currentMazeX]);
					Debug.LogFormat("[Mazematics #{0}] Current value changed from {1} --> {2}", _moduleId, currentValue - maze[currentMazeY][currentMazeX], currentValue);
					Debug.LogFormat("[Mazematics #{0}] SOLVED!", _moduleId);
					
					bombAudio.PlaySoundAtTransform("SolveSynth", transform);
					moduleStage = 3;
					GetComponent<KMBombModule>().HandlePass();
				}
				else if (currentValue < 0 || currentValue > 49)
				{
					Debug.LogFormat("[Mazematics #{0}] STRIKE! Out of bounds - maze will not move", _moduleId);
					Debug.LogFormat("[Mazematics #{0}] Attempted to move to: {1}{2} (cell is equal to {3})", _moduleId, logCoordinates[currentMazeX], (startMazeY + 1), maze[currentMazeY][currentMazeX]);
					Debug.LogFormat("[Mazematics #{0}] Current value would have changed from {1} --> {2}", _moduleId, currentValue - maze[currentMazeY][currentMazeX], currentValue);
					
					currentValue -= maze[currentMazeY][currentMazeX];
					currentMazeX -= x;
					currentMazeY -= y;

					currentMazeX = (currentMazeX + 8) % 8; 
					currentMazeY = (currentMazeY + 8) % 8;

					Debug.LogFormat("[Mazematics #{0}] Current value must be within 0-49. Maze not moved, coordinate is still {1}{2}, current value is still {3}", _moduleId, logCoordinates[currentMazeX], (startMazeY + 1), currentValue);

					GetComponent<KMBombModule>().HandleStrike();
				}
				else if (bannedValues.Contains(currentValue) == true)
				{
					Debug.LogFormat("[Mazematics #{0}] STRIKE! Restricted value - maze will still move", _moduleId);
					Debug.LogFormat("[Mazematics #{0}] Moved to: {1}{2} (cell is equal to {3})", _moduleId, logCoordinates[currentMazeX], (startMazeY + 1), maze[currentMazeY][currentMazeX]);
					Debug.LogFormat("[Mazematics #{0}] Current value changed from {1} --> {2}", _moduleId, currentValue - maze[currentMazeY][currentMazeX], currentValue);

					GetComponent<KMBombModule>().HandleStrike();
				}
				else
				{
					Debug.LogFormat("[Mazematics #{0}] Moved to: {1}{2} (cell is equal to {3})", _moduleId, logCoordinates[currentMazeX], (startMazeY + 1), maze[currentMazeY][currentMazeX]);
					Debug.LogFormat("[Mazematics #{0}] Current value changed from {1} --> {2}", _moduleId, currentValue - maze[currentMazeY][currentMazeX], currentValue);
				}
			}
			else
			{
				Debug.LogFormat("[Mazematics #{0}] Starting maze coordinate: {1}{2} (cell is equal to {3})", _moduleId, logCoordinates[startMazeX], (startMazeY + 1), maze[startMazeY][startMazeX]);
				Debug.LogFormat("[Mazematics #{0}] Current value changed from {1} --> {2}", _moduleId, startValue, currentValue - maze[currentMazeY][currentMazeX]);

				Debug.LogFormat("[Mazematics #{0}] Moved to: {1}{2} (cell is equal to {3})", _moduleId, logCoordinates[currentMazeX], (startMazeY + 1), maze[currentMazeY][currentMazeX]);
				Debug.LogFormat("[Mazematics #{0}] Current value changed from {1} --> {2}", _moduleId, currentValue - maze[currentMazeY][currentMazeX], currentValue);
			}

			movedOnce = true;
		}
		else
		{
			bombAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.SelectionTick, transform);
		}
		
		return false;
	}

	void Update () {
		
		int lastDigitOfTimer = Mathf.RoundToInt(Mathf.Floor((bombInfo.GetTime()) % 10));
		
		if (moduleStage == 0 || moduleStage == 3)
		{
			quadRenderer.enabled = true;
			screenTextRenderer.enabled = false;
			quadRenderer.material = testCard;
		}
		else if (moduleStage == 1)
		{
			quadRenderer.enabled = false;
			screenTextRenderer.enabled = true;
			screenTextMesh.text = currentValue.ToString();
		}
		else if (moduleStage == 2)
		{
			if (lastDigitOfTimer == 0)
			{
				quadRenderer.enabled = false;
				screenTextRenderer.enabled = true;
				screenTextMesh.text = goalValue.ToString();
			}
			else if (lastDigitOfTimer == leftover)
			{
				quadRenderer.enabled = false;
				screenTextRenderer.enabled = true;
				screenTextMesh.text = currentValue.ToString();
			}
			else
			{
				quadRenderer.enabled = true;
				screenTextRenderer.enabled = false;
				quadRenderer.material = shapeList[lastDigitOfTimer];
			}
		}
	}
	public string TwitchHelpMessage = "Use '!{0} press <button>' to press a button. Valid buttons are: up, down, left, right, screen. You can use u; d; l; r for the directions. (Please don't use capitals!) You can also chain the buttons. For ex. '{0} press screen up down display'";
    IEnumerator ProcessTwitchCommand(string command)
    {
		string commfinal=command.Replace("press ", "");
		string[] digitstring = commfinal.Split(' ');
		int tried;
		foreach(string option in digitstring){
			if(option=="up"){
				yield return null;
				yield return arrowUp;
				yield return arrowUp;
			}
			if(option=="down"){
				yield return null;
				yield return arrowDown;
				yield return arrowDown;
			}
			if(option=="left"){
				yield return null;
				yield return arrowLeft;
				yield return arrowLeft;
			}
			if(option=="right"){
				yield return null;
				yield return arrowRight;
				ield return arrowRight;
			}
			if(option=="display" || option=="screen"){
				yield return null;
				yield return screen;
				yield return screen;
			}
		}
	}
}
