// see: http://forum.unity3d.com/threads/116509-Improved-Terrain-Texture-Tiling 

using UnityEngine;
using System.Collections;

public class TerrainShader : MonoBehaviour {

	public float mnogitel = -0.125f; //Множитель
		
		
	void Aweke () 
	{
	
		Terrain terrainComp = (Terrain)GetComponent(typeof(Terrain));//Текущий террайн
		
		Shader.SetGlobalFloat("_Mnogitel", mnogitel);//Передаем значение множителя
				
		int prototypesLength = terrainComp.terrainData.splatPrototypes.Length;//Длина массива
		 
		for (int i = 0; i <= prototypesLength; i=i++)
         	{
			float TileX = terrainComp.terrainData.splatPrototypes[i].tileSize.x;
			float TileY = terrainComp.terrainData.splatPrototypes[i].tileSize.y;
			
				if (i<=3)
				{
						
					Debug.Log("i:"+i);
					Shader.SetGlobalFloat("_TileX"+i, TileX);
					Shader.SetGlobalFloat("_TileY"+i, TileY);
					
				} 
				if (i>=4 && i<=7)  
				{
					Debug.Log("i:"+i);
					int k = i-4;
					Debug.Log("k:"+k);
					Shader.SetGlobalFloat("_TileX"+k, TileX);
					Shader.SetGlobalFloat("_TileY"+k, TileY);
				} 
								
			}//for
		
		float terrainSizeX = terrainComp.terrainData.size.x;
		float terrainSizeZ = terrainComp.terrainData.size.z;
		
		Shader.SetGlobalFloat("_TerrainX", terrainSizeX);
		Shader.SetGlobalFloat("_TerrainZ", terrainSizeZ);
	}
	
	void Update ()
	{
			
	}
}
