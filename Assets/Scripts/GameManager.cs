using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	private const float LENGTH_OF_TERRAIN = 500f;
	private const float LENGTH_OF_ROADBLOCK = 10f;
	private const float WIDTH_OF_SIDEWALK = 2.5f;
	private const float RED_LIGHT_TIME = 3f;
	private const float GREEN_LIGHT_TIME = 3f;
	private const float YELLOW_LIGHT_TIME = 3f;
	private const float PEDESTRIAN_DENSITY = 0.3f;
	private const float WALKING_PED_SPEED = 5f;

	private const float PROBABILITY_OF_INTERSECTION_SPAWNING = 0.2f;		//dynamicity

	public GameObject playerCar;
	public GameObject terrain, roadBlock, roadBlockZebra, terrainReference, trafficLight, sideWalk, trafficLights, pedestrianPrefab, pedestrianWalkerPrefab;

	private Animator pedAnimator, pedAnimatorWalker;
	private List<Animator> listAnim = new List<Animator>();
	private List<GameObject> pedsWalk = new List<GameObject>();

	private GameObject currentTerrain, currentRoadModel, tempRoadBlock1, tempRoadBlock2, tempSideWalk;
	private int iteratorInstant=1, animIterator=0, walkingPeds=0;
	private Vector3 instantPos, terrainInstantPos;

	void Start () 
	{
		instantPos = new Vector3 (0f,0f,0f);
		terrainInstantPos = new Vector3 (-250f, 0f, -5f);
		instantiator ();
		greenLightActivators ();
	}

	void instantiator()									//Instantiates terrain, roads, sidewalks and intersections
	{
		//terrain
		currentTerrain = Instantiate (terrain, terrainInstantPos, Quaternion.identity);
		terrainInstantPos.z += LENGTH_OF_TERRAIN;
		currentTerrain.transform.SetParent (terrainReference.transform);

		//road and sidewalk
		currentRoadModel = roadBlock;
		for (int i = 0; i < LENGTH_OF_TERRAIN / LENGTH_OF_ROADBLOCK; i++) 
		{
			if (Random.Range (0f, 1f) <= PROBABILITY_OF_INTERSECTION_SPAWNING) 
			{
				currentRoadModel = roadBlockZebra;
				//traffic light
				GameObject redLight = Instantiate(trafficLight,instantPos-new Vector3(WIDTH_OF_SIDEWALK*(2*(LENGTH_OF_ROADBLOCK/WIDTH_OF_SIDEWALK)-1),0f,0f),Quaternion.identity);
				redLight.transform.Rotate (new Vector3 (0f, 180f, 0f));
				redLight.transform.SetParent (trafficLights.transform);

				//road crossing peds
				if (Random.Range (0f, 1f) <= PEDESTRIAN_DENSITY) 
				{
					createPeds (pedestrianPrefab, (instantPos - new Vector3 (WIDTH_OF_SIDEWALK * (2 * (LENGTH_OF_ROADBLOCK / WIDTH_OF_SIDEWALK) - 1), 0f, 0f)),redLight.transform,0);
				}
			} else {
				currentRoadModel = roadBlock;
			}

			//road
			create (currentRoadModel, instantPos, instantPos - new Vector3 (LENGTH_OF_ROADBLOCK, 0f, 0f), currentTerrain.transform,currentTerrain.transform);
			//sidewalk
			create (sideWalk, instantPos-new Vector3(WIDTH_OF_SIDEWALK*(2*(LENGTH_OF_ROADBLOCK/WIDTH_OF_SIDEWALK)-1),0f,0f), instantPos+new Vector3(WIDTH_OF_SIDEWALK*((LENGTH_OF_ROADBLOCK/WIDTH_OF_SIDEWALK)-1),0f,0f), tempRoadBlock1.transform,tempRoadBlock2.transform);

			//walking peds
			if (Random.Range (0f, 1f) <= PEDESTRIAN_DENSITY) 
			{
				Vector3 temp = (instantPos - new Vector3 (WIDTH_OF_SIDEWALK * (2 * (LENGTH_OF_ROADBLOCK / WIDTH_OF_SIDEWALK) - 1), 0f, 0f));
				if (Random.Range (0f, 1f) >= 0.5f)
					temp = instantPos + new Vector3 (WIDTH_OF_SIDEWALK * ((LENGTH_OF_ROADBLOCK / WIDTH_OF_SIDEWALK) - 1), 0f, 0f);
				createPeds (pedestrianWalkerPrefab, temp,tempRoadBlock1.transform,1);
			}

			instantPos.z += LENGTH_OF_ROADBLOCK;
		}

	}
		

	void createPeds (GameObject model, Vector3 abc, Transform parent1, int type)
	{
		float positionOffset=1f;
		for (int j = 0; j < Random.Range (1, 4); j++) 
		{
			//road crossing peds
			GameObject ped = Instantiate (model, abc+new Vector3(0f,0f,positionOffset++), Quaternion.identity);
			pedAnimator = ped.transform.GetChild(0).GetComponent<Animator> ();
			listAnim.Add (pedAnimator);
			animIterator++;
			//ped.transform.Rotate (new Vector3 (-90f, 0f, 0f));
			ped.transform.SetParent (parent1.transform);
			if (type==1) 
			{
				pedsWalk.Add (ped);
				walkingPeds++;
			}
		}
	}

	void create (GameObject model, Vector3 abc, Vector3 xyz, Transform parent1, Transform parent2)
	{
		GameObject temp1 = Instantiate (model, abc, Quaternion.identity);
		GameObject temp2 = Instantiate (model, xyz, Quaternion.identity);
		temp1.transform.SetParent (parent1);
		temp2.transform.SetParent (parent2);
		tempRoadBlock1 = temp1;
		tempRoadBlock2 = temp2;
	}
	
	void FixedUpdate () {

		if (playerCar.transform.position.z > iteratorInstant * LENGTH_OF_TERRAIN / 2) 
		{
			iteratorInstant += 2;
			instantiator ();
		}

		for (int i = 0; i < walkingPeds; i++) 
		{
			pedsWalk [i].transform.position += new Vector3 (0f,0f,WALKING_PED_SPEED*Time.deltaTime); 
		}

	}
		
	IEnumerator wait(float time)
	{
		return new WaitForSecondsRealtime (time);
	}

	void redLightActivators()
	{
		resetLights ();
		for(int i=0;i<trafficLights.transform.childCount;i++) 
		{
			trafficLights.transform.GetChild (i).transform.GetChild (2).gameObject.SetActive (true);
			trafficLights.transform.GetChild (i).transform.GetChild (5).gameObject.SetActive (true);
		}
		paramSetter ("pedState", 1);
		Invoke ("animReset", 2f);
		Invoke ("greenLightActivators", RED_LIGHT_TIME);
	}

	void animReset()
	{
		paramSetter ("pedState", 0);
	}

	void greenLightActivators()
	{
		resetLights ();
		for(int i=0;i<trafficLights.transform.childCount;i++) 
		{
			trafficLights.transform.GetChild (i).transform.GetChild (0).gameObject.SetActive (true);
			trafficLights.transform.GetChild (i).transform.GetChild (3).gameObject.SetActive (true);
		}
		animReset();
		Invoke ("yellowLightActivators", GREEN_LIGHT_TIME);
	}

	void yellowLightActivators()
	{
		resetLights ();
		for(int i=0;i<trafficLights.transform.childCount;i++) 
		{
			trafficLights.transform.GetChild (i).transform.GetChild (1).gameObject.SetActive (true);
			trafficLights.transform.GetChild (i).transform.GetChild (4).gameObject.SetActive (true);
		}
		animReset();
		Invoke ("redLightActivators", YELLOW_LIGHT_TIME);
	}

	void resetLights()
	{
		for(int i=0;i<trafficLights.transform.childCount;i++) 
		{
			for (int j = 0; j < 3; j++) 
			{
				trafficLights.transform.GetChild (i).transform.GetChild (j).gameObject.SetActive (false);
				trafficLights.transform.GetChild (i).transform.GetChild (j+3).gameObject.SetActive (false);
			}
		}
	}

	void paramSetter(string name, int state)
	{
		for (int i = 0; i < animIterator; i++) 
		{
			listAnim[i].SetInteger (name, state);
		}
	}
}
