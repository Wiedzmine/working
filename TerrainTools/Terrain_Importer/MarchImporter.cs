// Что бы читался русский текст в дебаге сохраните этот скрип в формате Windows 1251

using System.Globalization;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
//using System.Collections;

[ExecuteInEditMode]
public class MarchImporter : EditorWindow
{
    public GUI.WindowFunction WinFunc;
    // private Rect _winRect;

    // Controllers
    private bool _firstload = true;
    private bool _terrainFoldout = true;
    private bool _splatFoldout;
	private bool _normalFoldout;
    private bool _treeFoldout;
    private bool _grassFoldout;
    private bool _detailFoldout = true;
    private bool _optionFoldout = true;
    private bool _optionSplatMix = true;
    private Vector2 _scrollPosition = new Vector2(1, 1);
    private Vector2 _scrollPosition1 = new Vector2(1, 1);
   // private Vector2 _scrollPosition1a = new Vector2(1, 1);
    private Vector2 _scrollPosition2 = new Vector2(1, 1);
   // private Vector2 _scrollPosition2a = new Vector2(1, 1);
    private Vector2 _scrollPosition3 = new Vector2(1, 1);
   // private Vector2 _scrollPosition3a = new Vector2(1, 1);
    private Vector2 _scrollPosition4 = new Vector2(1, 1);
    private int _depthIndex = 1;
    private int _byteorderIndex;
    // Input fields
    private string _imagesPath = @"C:\SevenHeven\WorldMachine\257";
    private string _heightmapMask = "hm_x{0}_y{1}";
    private string _terrainMask = "terrain_x{0}_y{1}";
    private Vector2 _heightmapsize = new Vector2(257, 257);
    private Vector3 _terrainsize = new Vector3(256, 256, 256);

    private readonly System.Collections.Generic.List<SplatMapInfo2> _splats =
        new System.Collections.Generic.List<SplatMapInfo2>();
		
	private readonly System.Collections.Generic.List<NormalMapInfo2> _normals =
        new System.Collections.Generic.List<NormalMapInfo2>();	

    private readonly System.Collections.Generic.List<TreeMapInfo2> _trees =
        new System.Collections.Generic.List<TreeMapInfo2>();

    private readonly System.Collections.Generic.List<GrassInfo2> _grass =
        new System.Collections.Generic.List<GrassInfo2>();

    private int _heightmaperror = 7;
    private int _basemapdistance = 1000;
    private bool _castShadows = true;
    private Material _mat = null;
 
    private float _detailObjectDistance = 40f;
    private float _detailObjectDensity = 0.5f;
    private float _treeDistance = 2000f;
    private float  _treeBillboardDistance = 100f;
    private float _treeCrossFadeLength = 20f;
    private int _treeMaximumFullLodCount = 200; //Количество деревьев которые будут показаны при полном лоде
    
    private float _windspeed = 0.5f;
    private float _windsize = 0.5f;
    private float _bendfactor = 0.5f;
    private Color _grasstint = new Color(0.698f, 0.6f, 0.5f);

    private float _progress;
    private bool _generateBundle;
    private bool _generateTerrain = true;
    private string _errorString = "Все прошло успешно.";
   // private float _ratioleft = 0.5f;
   // private float _ratioright = 0.5f;
    private float _optionSliderMix = 50f;
     
    // Use this for initialization
    [MenuItem("Tools/MarchImporter")]
    private static void Init()
    {

        MarchImporter window = (MarchImporter)EditorWindow.GetWindow(typeof(MarchImporter));
        window.Show();
    }

    private void OnGUI()
    {
       
       _errorString = "Все прошло успешно.";
        Texture2D icoTexture = new Texture2D(24, 24); //создаем текстуру
         
        //путь до текстуры
        icoTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Editor/MarchImporter/ico/minecraft24.psd", typeof(Texture2D));
        EditorGUI.DrawPreviewTexture(new Rect(7, 5, 24, 24), icoTexture);
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        //Создадим лейбл с названием раздела "Terrain Import Settings"
        GUILayout.Label("        Import Terrain v.0.1", EditorStyles.boldLabel, GUILayout.Width(165), GUILayout.Height(19));
        GUILayout.BeginVertical();
        GUILayout.Space(6);
        GUILayout.Label("-  Создайте в WorldMachine2 карты и экпортируйте.");
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        //Начало главного скрола
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        
        
        // Начало обромления Terrain Import Settings
        GUILayout.BeginHorizontal("box");
        GUILayout.BeginVertical();
        
        EditorGUILayout.BeginHorizontal();
        //путь до текстуры
        icoTexture = (Texture2D) AssetDatabase.LoadAssetAtPath("Assets/Editor/MarchImporter/ico/mount.psd", typeof (Texture2D));
        EditorGUI.DrawPreviewTexture(new Rect(20, 8, 16, 16), icoTexture);
        // Выпадающий список
        _terrainFoldout = EditorGUILayout.Foldout(_terrainFoldout, "     Terrain Import Settings");
        EditorGUILayout.EndHorizontal();

        

        if (_terrainFoldout)
        {
            GUILayout.BeginHorizontal("box");
            GUILayout.BeginVertical();
            // Images Path
            // Создадаим текстовое поле до папки с файлами
            _imagesPath = EditorGUILayout.TextField("Images Patch", _imagesPath);
            // Heightmap Mask
            // Имя и маска Height-мапы
            _heightmapMask = EditorGUILayout.TextField("Heightmap File Mask", _heightmapMask);
            // Terrain Asset Mask
            // Название файлов как они будут отображаться в папке Ассетов
            _terrainMask = EditorGUILayout.TextField("Terrain Asset Mask", _terrainMask);
            // Heighmap depth
            // Разрядность 16-bit файла Height-мапы 
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Depth", GUILayout.Width(40), GUILayout.Height(21));
            _depthIndex = EditorGUILayout.Popup(_depthIndex,new[]{new GUIContent("8-Bit", "8-Bit"),new GUIContent("16-bit", "16-bit")}, GUILayout.Width(120), GUILayout.Height(21));
            // Heighmap byte order
            // Формат файла местности (Height-мапы)
            GUILayout.Label("Byte Order", GUILayout.Width(70), GUILayout.Height(21));
            _byteorderIndex = EditorGUILayout.Popup(_byteorderIndex,new[]{new GUIContent("Windows", "Windows"),new GUIContent("Mac", "Mac")},new[] {GUILayout.Width(120), GUILayout.Height(21)});
            EditorGUILayout.EndHorizontal();
            // Heighmap size
            // Размер читаемой Height-мапы
            _heightmapsize = EditorGUILayout.Vector2Field("Heightmap Size:", _heightmapsize);
            //Terrain size
            // Размер создоваемого Terraina
            _terrainsize = EditorGUILayout.Vector3Field("Terrain Size:", _terrainsize);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
        //Конец обромление Terrain Import Settings
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        
    
        //Создадим лейбл с названием раздела ""Splats, Trees and Details"
        GUILayout.Label("Splats, Trees and Details", EditorStyles.boldLabel);

    
        // Terrain splatmask panel
        // Начало обромления Splat
        GUILayout.BeginHorizontal("box");
        GUILayout.BeginVertical();

        //Начало скрола Splate (Общий)
        _scrollPosition1 = EditorGUILayout.BeginScrollView(_scrollPosition1);
        EditorGUILayout.BeginHorizontal();
        icoTexture = (Texture2D) AssetDatabase.LoadAssetAtPath("Assets/Editor/MarchImporter/ico/map.psd", typeof (Texture2D));
        EditorGUI.DrawPreviewTexture(new Rect(16, 1, 16, 16), icoTexture);
        _splatFoldout = EditorGUILayout.Foldout(_splatFoldout, "     Terrain Splatmaps");
        EditorGUILayout.EndHorizontal();
       
        if (_splatFoldout)
        {
            
            GUILayout.BeginHorizontal("box");
            //Начало скрола Splate
            //_scrollPosition1a = EditorGUILayout.BeginScrollView(_scrollPosition1a);
            GUILayout.BeginVertical();

            // Кнопка New Splatmap
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            if (GUILayout.Button("Новая Splatmap", GUILayout.Width(120), GUILayout.Height(21)))
            {
                AddNewSplatmap();
            }
            EditorGUILayout.EndHorizontal();
            //Конец кнопки

            for (int i = 0; i < _splats.Count; i++)
            {
                // Generate the splat information.
                SplatMapInfo2 splat = _splats[i];
				NormalMapInfo2 normal = _normals[i];
				

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                splat.FoldOutStat = EditorGUILayout.Foldout(splat.FoldOutStat, "SplatMap " + (i + 1).ToString());
                if (GUILayout.Button("Удалить Splatmap", GUILayout.Width(120), GUILayout.Height(21)))
                {
                    DeleteSplatmap(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
                
                if (splat.FoldOutStat)
                 
                {   
                    //* Add delete splat button
                    //Базовая текстура и нормаль
					GUILayout.Label("Текстура:               Бамп:", GUILayout.Width(190), GUILayout.Height(17));
					
					GUILayout.BeginHorizontal("box");
               
                    splat.Texture =(Texture2D) EditorGUILayout.ObjectField(splat.Texture, typeof (Texture2D), false, GUILayout.Width(110), GUILayout.Height(110));
					normal.Texture =(Texture2D) EditorGUILayout.ObjectField(normal.Texture, typeof (Texture2D), false, GUILayout.Width(110), GUILayout.Height(110));
										
               GUILayout.BeginVertical();
                    
                    EditorGUILayout.LabelField("Tile Size:", GUILayout.Width(80));
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("X:", GUILayout.Width(15));
                    splat.TileSizeX = EditorGUILayout.IntField(splat.TileSizeX);
					normal.TileSizeX = splat.TileSizeX;
                    GUILayout.Label("Y:", GUILayout.Width(15));
                    splat.TileSizeY = EditorGUILayout.IntField(splat.TileSizeY);
					normal.TileSizeY = splat.TileSizeY;
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.LabelField("Tile Offset:", GUILayout.Width(80));
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("X:", GUILayout.Width(15));
                    splat.TileOffsetX = EditorGUILayout.IntField(splat.TileOffsetX);
					normal.TileOffsetX = splat.TileOffsetX;
                    GUILayout.Label("Y:", GUILayout.Width(15));
                    splat.TileOffsetY = EditorGUILayout.IntField(splat.TileOffsetY);
					normal.TileOffsetY = splat.TileOffsetY;
                    EditorGUILayout.EndHorizontal();

                    
                    GUILayout.Label("Filename Mask:");
                    EditorGUILayout.BeginHorizontal();
                    splat.Filemask = EditorGUILayout.TextArea(splat.Filemask);
					normal.Filemask = splat.Filemask;
                    EditorGUILayout.EndHorizontal();

              GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                   

                }
                // Add back the splat object to the list
                _splats[i] = splat;
				_normals[i] = normal;
				
            }

               GUILayout.EndVertical();
            //Конец скрола Splate
           //EditorGUILayout.EndScrollView();
           GUILayout.EndHorizontal();
           }
        //Конец скрола Splate (Общий)
       EditorGUILayout.EndScrollView();
        //  Конец обромления Splat
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        


        //{ Tree definition
        // Начало обромления Tree
        GUILayout.BeginHorizontal("box");
        GUILayout.BeginVertical();

        //Начало скрола Tree (Общий)
        _scrollPosition2 = EditorGUILayout.BeginScrollView(_scrollPosition2);
        
        EditorGUILayout.BeginHorizontal();
        icoTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Editor/MarchImporter/ico/tree.psd", typeof(Texture2D));
        EditorGUI.DrawPreviewTexture(new Rect(16, 1, 16, 16), icoTexture);
      _treeFoldout = EditorGUILayout.Foldout(_treeFoldout, "     Tree Splatmaps");
        EditorGUILayout.EndHorizontal();

        
        if (_treeFoldout)
        {
            GUILayout.BeginHorizontal("box");
            //Начало скрола Tree
         //   _scrollPosition2a = EditorGUILayout.BeginScrollView(_scrollPosition2a);
            GUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            if (GUILayout.Button("New Tree map", GUILayout.Width(120), GUILayout.Height(21)))
            {
                AddNewTreemap();
            }
            EditorGUILayout.EndHorizontal();
            for (int i = 0; i < _trees.Count; i++)
            {
                // Generate the splat information.
                TreeMapInfo2 tree = _trees[i];

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                tree.FoldOutStat = EditorGUILayout.Foldout(tree.FoldOutStat, "Tree Map " + (i + 1).ToString(CultureInfo.InvariantCulture));
                // Add delete splat button

                if (GUILayout.Button("Delete Tree Map", GUILayout.Width(120), GUILayout.Height(21)))
                {
                    DeleteTreemap(i);
                    break;
                }
                
                EditorGUILayout.EndHorizontal();

                if (tree.FoldOutStat)
                {

                    GUILayout.Space(20);
                    // Add Object field
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Tree Prefab:", GUILayout.Width(100), GUILayout.Height(17));
                    tree.Prefab = (GameObject) EditorGUILayout.ObjectField(tree.Prefab, typeof (GameObject), false);
                    EditorGUILayout.EndHorizontal();

                    // Add bend Factor field
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Bend Factor:", GUILayout.Width(100), GUILayout.Height(17));
                    tree.BendFactor = EditorGUILayout.IntField(tree.BendFactor);
                    EditorGUILayout.EndHorizontal();

                    // Add color variation field
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Color Variation:",  GUILayout.Width(100), GUILayout.Height(17));
                    tree.ColorVariation = EditorGUILayout.Slider(tree.ColorVariation, 0, 1);                                             
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Height Scale:",  GUILayout.Width(100), GUILayout.Height(17));
                    tree.HeightScale = EditorGUILayout.IntSlider(tree.HeightScale, 50, 200);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    
                    GUILayout.Label("Height Variation:", GUILayout.Width(100), GUILayout.Height(17));
                    tree.HeightVariation = EditorGUILayout.IntSlider(tree.HeightVariation, 0, 30);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Width Scale:", GUILayout.Width(100), GUILayout.Height(17));
                    tree.WidthScale = EditorGUILayout.IntSlider(tree.WidthScale, 50, 200);                                    
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Width Variation:", GUILayout.Width(100), GUILayout.Height(17));
                    tree.WidthVariation = EditorGUILayout.IntSlider(tree.WidthVariation, 0, 30);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Max Density:", GUILayout.Width(100), GUILayout.Height(17));
                    tree.MaxDensity = EditorGUILayout.Slider(tree.MaxDensity, 0, 1);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Filename Mask:", GUILayout.Width(100), GUILayout.Height(17));
                    tree.Filemask = EditorGUILayout.TextArea(tree.Filemask);
                    EditorGUILayout.EndHorizontal();
                }
                // Add back the tree object to the list
                _trees[i] = tree;
            }
            GUILayout.EndVertical();
            //Конец скрола Tree
            //GUILayout.EndScrollView();
            GUILayout.EndHorizontal();

        }
       
        //Конец скрола Tree (Общий)
       EditorGUILayout.EndScrollView();
        //}  Конец обромления Tree
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        
      
     


        //{ Начало обромления Травы
        GUILayout.BeginHorizontal("box");
        GUILayout.BeginVertical();

        // Начало скрола Травы
        _scrollPosition3 = EditorGUILayout.BeginScrollView(_scrollPosition3);

        EditorGUILayout.BeginHorizontal();
        icoTexture = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Editor/MarchImporter/ico/grass.psd", typeof(Texture2D));
        EditorGUI.DrawPreviewTexture(new Rect(16, 1, 16, 16), icoTexture);
        _grassFoldout = EditorGUILayout.Foldout(_grassFoldout, "    Grass Splatmaps");
        EditorGUILayout.EndHorizontal();
        
        // Grass Foldout
        if (_grassFoldout)
        {

            GUILayout.BeginHorizontal("box");
           //Начало скрола Grass
           // _scrollPosition3a = EditorGUILayout.BeginScrollView(_scrollPosition3a);
            GUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            if (GUILayout.Button("New Grass Map", GUILayout.Width(120), GUILayout.Height(21)))
            {
                AddNewGrassmap();
            }
            EditorGUILayout.EndHorizontal();
            for (int i = 0; i < _grass.Count; i++)
            {
                // Generate the splat information.
                GrassInfo2 gr = _grass[i];

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                gr.FoldOutStat = EditorGUILayout.Foldout(gr.FoldOutStat,"Grass Map " + (i + 1).ToString(CultureInfo.InvariantCulture));
                // Add delete splat button
                if (GUILayout.Button("Delete Grass Map", new[] { GUILayout.Width(120), GUILayout.Height(21) }))
                {
                    DeleteGrassmap(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
                if (gr.FoldOutStat)
                {
                  
                    GUILayout.Label("Текстура:", GUILayout.Width(100), GUILayout.Height(17));
                   
             EditorGUILayout.BeginHorizontal();
                    gr.Texture = (Texture2D) EditorGUILayout.ObjectField(gr.Texture, typeof (Texture2D), false, GUILayout.Width(100), GUILayout.Height(100));
             GUILayout.BeginVertical();

           

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Min Width:", GUILayout.Width(100), GUILayout.Height(17));
                    gr.MinWidth = EditorGUILayout.FloatField(gr.MinWidth);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Max Width:", GUILayout.Width(100), GUILayout.Height(17));
                    gr.MaxWidth = EditorGUILayout.FloatField(gr.MaxWidth);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Min Height:", GUILayout.Width(100), GUILayout.Height(17));
                    gr.MinHeight = EditorGUILayout.FloatField(gr.MinHeight);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Max Height:", GUILayout.Width(100), GUILayout.Height(17));
                    gr.MaxHeight = EditorGUILayout.FloatField(gr.MaxHeight);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Noise Spread:", GUILayout.Width(100), GUILayout.Height(17));
                    gr.NoiseSpread = EditorGUILayout.FloatField(gr.NoiseSpread);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Grass Density:", GUILayout.Width(100), GUILayout.Height(17));
                    gr.GrassDensity = EditorGUILayout.Slider(gr.GrassDensity, 0, 10);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Healthy Color:", GUILayout.Width(100), GUILayout.Height(17));
                    gr.HealthyColor = EditorGUILayout.ColorField(gr.HealthyColor);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Dry Color:", GUILayout.Width(100), GUILayout.Height(17));
                    gr.DryColor = EditorGUILayout.ColorField(gr.DryColor);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Billboard:", GUILayout.Width(100), GUILayout.Height(17));
                    gr.Billboard = EditorGUILayout.Toggle(gr.Billboard);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Filename Mask:",GUILayout.Width(100), GUILayout.Height(17));
                    gr.Filemask = EditorGUILayout.TextArea(gr.Filemask);
                    EditorGUILayout.EndHorizontal();

             GUILayout.EndVertical();
             EditorGUILayout.EndHorizontal();
                  
                }
                // Add back the tree object to the list
                _grass[i] = gr;
            }
            GUILayout.EndVertical();
            //Конец скрола Grass
            //GUILayout.EndScrollView();
            GUILayout.EndHorizontal();
         }
        //Конец скрола травы
       EditorGUILayout.EndScrollView();
        //} Конец обромления Травы
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();



        _detailFoldout = EditorGUILayout.Foldout(_detailFoldout, "Настройки");

        if (_detailFoldout)
        {

            //{ Global Properties
            GUILayout.BeginHorizontal("box");
            GUILayout.BeginVertical();
            GUILayout.Label("Base Terrain", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Pixel Error", GUILayout.Width(120), GUILayout.Height(17));
            _heightmaperror = EditorGUILayout.IntSlider(_heightmaperror, 1, 200);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Base Map Dist.", GUILayout.Width(120), GUILayout.Height(17));
            _basemapdistance = EditorGUILayout.IntSlider(_basemapdistance, 0, 2000);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Cast Shadow", GUILayout.Width(120), GUILayout.Height(17));
            _castShadows = EditorGUILayout.Toggle(_castShadows);
            EditorGUILayout.EndHorizontal();

        
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Material", GUILayout.Width(120), GUILayout.Height(17));
                _mat = (Material) EditorGUILayout.ObjectField(_mat, typeof (Material), false);
                EditorGUILayout.EndHorizontal();
            

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Detail Destance", GUILayout.Width(120), GUILayout.Height(17));
            _detailObjectDistance = EditorGUILayout.Slider( _detailObjectDistance, 0, 250);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Detail Density", GUILayout.Width(120), GUILayout.Height(17));
            _detailObjectDensity = EditorGUILayout.Slider( _detailObjectDensity, 0, 1);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Tree Destance", GUILayout.Width(120), GUILayout.Height(17));
            _treeDistance = EditorGUILayout.Slider(_treeDistance, 5, 2000);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Billboard Start", GUILayout.Width(120), GUILayout.Height(17));
            _treeBillboardDistance = EditorGUILayout.Slider(_treeBillboardDistance, 5, 2000);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Fade Lenght", GUILayout.Width(120), GUILayout.Height(17));
            _treeCrossFadeLength = EditorGUILayout.Slider(_treeCrossFadeLength, 0, 200);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();//Количество деревьев которые будут показаны при полном лоде
            GUILayout.Label("Max Mesh Trees", GUILayout.Width(120), GUILayout.Height(17));
            _treeMaximumFullLodCount = EditorGUILayout.IntSlider(_treeMaximumFullLodCount, 0, 10000);
            EditorGUILayout.EndHorizontal();
           
            
            GUILayout.Label("Опции ветра", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Скорость", GUILayout.Width(120), GUILayout.Height(17));
            _windspeed = EditorGUILayout.Slider(_windspeed, 0, 1);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Size", GUILayout.Width(120), GUILayout.Height(17));
            _windsize = EditorGUILayout.Slider(_windsize, 0, 1);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Bending", GUILayout.Width(120), GUILayout.Height(17));
            _bendfactor = EditorGUILayout.Slider(_bendfactor, 0, 1);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Grass Tint:", GUILayout.Width(120), GUILayout.Height(17));
            _grasstint = EditorGUILayout.ColorField(_grasstint);
            EditorGUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

        }
		//}

        // Начало скрола
        //{ Начало обромления Option
        GUILayout.BeginHorizontal("box");
        GUILayout.BeginVertical();
        _scrollPosition4 = EditorGUILayout.BeginScrollView(_scrollPosition4, GUILayout.Height(120), GUILayout.MaxWidth(600));

        Texture2D icoTexture2 = new Texture2D(1, 1); //создаем текстуру
        icoTexture2 = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Editor/MarchImporter/ico/Option.psd", typeof(Texture2D));
        //путь до текстуры
        EditorGUI.DrawPreviewTexture(new Rect(16, 0, 16, 16), icoTexture2);
       
        _optionFoldout = EditorGUILayout.Foldout(_optionFoldout, "     Option");
       
        if (_optionFoldout)
        {
            EditorGUILayout.BeginHorizontal("box");
            GUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Выкинуть в сцену Terrain?", GUILayout.MaxWidth(200));
            _generateTerrain = EditorGUILayout.Toggle(_generateTerrain);
            GUILayout.Label("Создать Asset Bundle?", GUILayout.MaxWidth(200));
            _generateBundle = EditorGUILayout.Toggle(_generateBundle);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Нормализовать SplatMaps?", GUILayout.MaxWidth(200));
            _optionSplatMix = EditorGUILayout.Toggle(_optionSplatMix);
            GUILayout.Label("Пустая опция", GUILayout.MaxWidth(200));
            _optionSplatMix = EditorGUILayout.Toggle(_optionSplatMix);

            GUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Смешение");
            _optionSliderMix = EditorGUILayout.Slider(_optionSliderMix, 0, 1);
           // _ratioright = _optionSliderMix;
           // _ratioleft = 1 - _optionSliderMix;
            EditorGUILayout.EndHorizontal();
            GUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            

         }
        
      
        GUILayout.BeginHorizontal("box" , GUILayout.Height(25));
        
        if (GUILayout.Button("Импортировать террейны" , GUILayout.Height(21))) //Рисуем кнопку
        {
            ImportTerrains(true);
        }
      
        if (GUILayout.Button("Загрузить ini файл", GUILayout.Height(21))) //Рисуем кнопоку
        {
            LoadOption(true);
        }
   
        if (GUILayout.Button("Сохранить ini файл", GUILayout.Height(21))) //Рисуем кнопоку
        {
            SaveOption(true);
        }
       
        GUILayout.EndHorizontal();

       EditorGUILayout.EndScrollView();
	   //} Конец скрола Option
        // Конец обромления Terrain Import Settings
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView(); // Конец главного скрола  

        if (_firstload)
        {
            LoadOption(true);
        }
    }

    // Импорт террайнов
    private void ImportTerrains(bool key)
    {
      
        if (key)
        {
            string info = VerifyImportData();
            if (string.IsNullOrEmpty(info))
            {
                string result = ImportTerrain();
                EditorUtility.DisplayDialog("March Terrain Importer", result, "Ok");
            }
            else
            {
                EditorUtility.DisplayDialog("Missing Information", info, "Ok");
            }
        }
    }

    //Кнопка: Загрузка с файла настроек
    private void LoadOption(bool key)
    {
        _firstload = false;
        if (key)
        {
            //Удалим все splatmap в окне перед загрузкой в это окно
            while (_splats.Count > 0)
            {
                _splats.RemoveAt(_splats.Count - 1);
            }
			while (_normals.Count > 0)
            {
                _normals.RemoveAt(_normals.Count - 1);
            }
            //Удалим все _trees в окне перед загрузкой в это окно
            while (_trees.Count > 0)
            {
                _trees.RemoveAt(_trees.Count - 1);
            }
            //Удалим все _grass в окне перед загрузкой в это окно
            while (_grass.Count > 0)
            {
                _grass.RemoveAt(_grass.Count - 1);
            }
           
            // Загрузка
            var textReader = new StreamReader(@"option.ini");

            while (textReader.Peek() != -1)
            {
                String line = textReader.ReadLine(); //Line = textReader.ReadToEnd();

                switch (line)
                {
                    case "<SplatMapInfo2>":

                        SplatMapInfo2 sp = new SplatMapInfo2();
                        sp.Texture = (Texture2D) AssetDatabase.LoadAssetAtPath(textReader.ReadLine(), typeof (Texture2D));
                        sp.Texture = (Texture2D) EditorGUILayout.ObjectField(sp.Texture, typeof (Texture2D), false);
                        sp.TileSizeX = Convert.ToInt32(textReader.ReadLine());
                        sp.TileSizeY = Convert.ToInt32(textReader.ReadLine());
                        sp.TileOffsetX = Convert.ToInt32(textReader.ReadLine());
                        sp.TileOffsetY = Convert.ToInt32(textReader.ReadLine());
                        sp.Filemask = textReader.ReadLine();
                        sp.FoldOutStat = true;
                        _splats.Add(sp);
                        _splatFoldout = true;
                        break;
					
					 case "<NormalMapInfo2>":
					 
						NormalMapInfo2 nm = new NormalMapInfo2();
                        nm.Texture = (Texture2D) AssetDatabase.LoadAssetAtPath(textReader.ReadLine(), typeof (Texture2D));
                        nm.Texture = (Texture2D) EditorGUILayout.ObjectField(nm.Texture, typeof (Texture2D), false);
                        nm.TileSizeX = Convert.ToInt32(textReader.ReadLine());
                        nm.TileSizeY = Convert.ToInt32(textReader.ReadLine());
                        nm.TileOffsetX = Convert.ToInt32(textReader.ReadLine());
                        nm.TileOffsetY = Convert.ToInt32(textReader.ReadLine());
                        nm.Filemask = textReader.ReadLine();
                        nm.FoldOutStat = true;
                        _normals.Add(nm);
                        _normalFoldout = true;
                        break;
					
                    case "<TreeMapInfo2>":

                      TreeMapInfo2 tm = new TreeMapInfo2();
                      tm.Prefab = (GameObject)AssetDatabase.LoadAssetAtPath(textReader.ReadLine(), typeof(GameObject));
                      tm.Prefab = (GameObject)EditorGUILayout.ObjectField(tm.Prefab, typeof(GameObject), false);
                      tm.BendFactor = Convert.ToInt32(textReader.ReadLine());
                      tm.ColorVariation = float.Parse(textReader.ReadLine());
                      tm.HeightScale = Convert.ToInt32(textReader.ReadLine());
                      tm.WidthScale = Convert.ToInt32(textReader.ReadLine());
                      tm.HeightVariation = Convert.ToInt32(textReader.ReadLine());
                      tm.WidthVariation = Convert.ToInt32(textReader.ReadLine());
                      tm.MaxDensity = float.Parse(textReader.ReadLine());
                      tm.Filemask = textReader.ReadLine();
                      tm.FoldOutStat = true;
                      _trees.Add(tm);
                      _treeFoldout = true;
                        break;
                       

                    case "<GrassInfo2>":

                       GrassInfo2 gi = new GrassInfo2();
                       gi.Texture = (Texture2D) AssetDatabase.LoadAssetAtPath(textReader.ReadLine(), typeof (Texture2D));
                       gi.Texture = (Texture2D) EditorGUILayout.ObjectField(gi.Texture, typeof (Texture2D), false);
                       gi.MinWidth = float.Parse(textReader.ReadLine());
                       gi.MaxWidth = float.Parse(textReader.ReadLine());
                       gi.MinHeight = float.Parse(textReader.ReadLine());
                       gi.MaxHeight = float.Parse(textReader.ReadLine());
                       gi.NoiseSpread = float.Parse(textReader.ReadLine());
                       gi.GrassDensity = float.Parse(textReader.ReadLine());

                       // читаем цвет gi.HealthyColor
                       string templine3 = textReader.ReadLine();
                       string[] parts3 = templine3.Split(","[0]);
                       float xx = float.Parse(parts3[0]);
                       float yy = float.Parse(parts3[1]);
                       float zz = float.Parse(parts3[2]);
                       gi.HealthyColor = new Color(xx, yy, zz);

                       // читаем цвет DryColor
                       string templine4 = textReader.ReadLine();
                       string[] parts4 = templine4.Split(","[0]);
                       float xx1 = float.Parse(parts4[0]);
                       float yy1 = float.Parse(parts4[1]);
                       float zz1 = float.Parse(parts4[2]);
                       gi.DryColor = new Color(xx1, yy1, zz1);
                       gi.RenderMode = DetailRenderMode.GrassBillboard; // Этот параметр просто запишем т.к. мы его в файл не писали
                       gi.Billboard = Convert.ToBoolean(textReader.ReadLine());
                       gi.Filemask = textReader.ReadLine();
                       gi.FoldOutStat = true;
                       _grass.Add(gi);
                       _grassFoldout = true;
                        
                       break;
      
                  case "<TerrainOption>":

                        _imagesPath = textReader.ReadLine(); // подгрузим путь до файлов 
                        _heightmapMask = textReader.ReadLine(); // подгрузим имя маски 
                        _terrainMask = textReader.ReadLine(); // подгрузим имя маски
                        _depthIndex = Convert.ToInt32(textReader.ReadLine()); // подгрузим разрядность террайна
                        _byteorderIndex = Convert.ToInt32(textReader.ReadLine()); // подгрузим еще какуюто хрень

                        // читаем размер террайна
                        string templine = textReader.ReadLine();
                        string[] parts = templine.Split(","[0]);
                        float x = float.Parse(parts[0]);
                        float y = float.Parse(parts[1]);
                        _heightmapsize = new Vector3(x, y);

                        // читаем цвет 
                        string templine1 = textReader.ReadLine();
                        string[] parts1 = templine1.Split(","[0]);
                        float x1 = float.Parse(parts1[0]);
                        float y1 = float.Parse(parts1[1]);
                        float z1 = float.Parse(parts1[2]);
                        _terrainsize = new Vector3(x1, y1, z1);

                        _heightmaperror = Convert.ToInt32(textReader.ReadLine());   // Точность карты
                        _basemapdistance = Convert.ToInt32(textReader.ReadLine());  // Дальность отрисовки карты
                        _castShadows = Convert.ToBoolean(textReader.ReadLine());    // Показывать тень
                        _mat = (Material)AssetDatabase.LoadAssetAtPath(textReader.ReadLine(), typeof(Material));
                        _mat = (Material)EditorGUILayout.ObjectField(_mat, typeof(Material), false);
                        _detailObjectDistance = float.Parse(textReader.ReadLine()); // На каком растоянии Detail объекты будут отображаться на расстоянии.
                        _detailObjectDensity = float.Parse(textReader.ReadLine());
                        _treeDistance = float.Parse(textReader.ReadLine());
                        _treeBillboardDistance = float.Parse(textReader.ReadLine());
                        _treeCrossFadeLength = float.Parse(textReader.ReadLine());
                        _treeMaximumFullLodCount = Convert.ToInt32(textReader.ReadLine());

                        _windspeed = float.Parse(textReader.ReadLine());  // скорость ветра
                        _windsize = float.Parse(textReader.ReadLine());   // размер ветра
                        _bendfactor = float.Parse(textReader.ReadLine()); // тоже ветра

                        // читаем цвет
                        string templine2 = textReader.ReadLine();
                        string[] parts2 = templine2.Split(","[0]);
                        float x2 = float.Parse(parts2[0]);
                        float y2 = float.Parse(parts2[1]);
                        float z2 = float.Parse(parts2[2]);
                        _grasstint = new Color(x2, y2, z2);
                        break;
                }

            }
            textReader.Close();
            //EditorGUILayout.EndHorizontal();
        }
    }

    //Кнопка: Сохранение в файл настроек
    private void SaveOption(bool key)
    {
        //Спасем и сохраним
        if (key)
        {
            // Запись в файл

            var textWriter = new StreamWriter(@"option.ini");
            if (_splats.Count > 0)
            {
                // SplatMapInfo2
                for (int i = 0; i < _splats.Count; i++)
                {
                    textWriter.WriteLine("<SplatMapInfo2>");
                    string patch = AssetDatabase.GetAssetPath(_splats[i].Texture);
                    textWriter.WriteLine(patch);
                    textWriter.WriteLine(_splats[i].TileSizeX);
                    textWriter.WriteLine(_splats[i].TileSizeY);
                    textWriter.WriteLine(_splats[i].TileOffsetX);
                    textWriter.WriteLine(_splats[i].TileOffsetY);
                    textWriter.WriteLine(_splats[i].Filemask);

                }
            }
			
			if (_normals.Count > 0)
            {
                // NormalMapInfo2
                for (int i = 0; i < _normals.Count; i++)
                {
                    textWriter.WriteLine("<NormalMapInfo2>");
                    string patch = AssetDatabase.GetAssetPath(_normals[i].Texture);
                    textWriter.WriteLine(patch);
                    textWriter.WriteLine(_normals[i].TileSizeX);
                    textWriter.WriteLine(_normals[i].TileSizeY);
                    textWriter.WriteLine(_normals[i].TileOffsetX);
                    textWriter.WriteLine(_normals[i].TileOffsetY);
                    textWriter.WriteLine(_normals[i].Filemask);

                }
            }
			
            if (_trees.Count > 0)
            {
                //TreeMapInfo2
                for (int i = 0; i < _trees.Count; i++)
                {
                    textWriter.WriteLine("<TreeMapInfo2>");
                    string patch = AssetDatabase.GetAssetPath(_trees[i].Prefab);
                    textWriter.WriteLine(patch);
                    textWriter.WriteLine(_trees[i].BendFactor);
                    textWriter.WriteLine(_trees[i].ColorVariation);
                    textWriter.WriteLine(_trees[i].HeightScale);
                    textWriter.WriteLine(_trees[i].WidthScale);
                    textWriter.WriteLine(_trees[i].HeightVariation);
                    textWriter.WriteLine(_trees[i].WidthVariation);
                    textWriter.WriteLine(_trees[i].MaxDensity);
                    textWriter.WriteLine(_trees[i].Filemask);
                }
            }
            if (_grass.Count > 0)
            {
                // GrassInfo2
                for (int i = 0; i < _grass.Count; i++)
                {
                    textWriter.WriteLine("<GrassInfo2>");
                    string patch = AssetDatabase.GetAssetPath(_grass[i].Texture);
                    textWriter.WriteLine(patch);
                    textWriter.WriteLine(_grass[i].MinWidth);
                    textWriter.WriteLine(_grass[i].MaxWidth);
                    textWriter.WriteLine(_grass[i].MinHeight);
                    textWriter.WriteLine(_grass[i].MaxHeight);
                    textWriter.WriteLine(_grass[i].NoiseSpread);
                    textWriter.WriteLine(_grass[i].GrassDensity);

                    // Запишим _grass.HealthyColor
                    string xx = float.Parse(_grass[i].HealthyColor[0].ToString()).ToString();
                    string yy = float.Parse(_grass[i].HealthyColor[1].ToString()).ToString();
                    string zz = float.Parse(_grass[i].HealthyColor[2].ToString()).ToString();
                    textWriter.WriteLine(xx + "," + yy + "," + zz);

                    // Запишим _grass.DryColor
                    string xx1 = float.Parse(_grass[i].DryColor[0].ToString()).ToString();
                    string yy1 = float.Parse(_grass[i].DryColor[1].ToString()).ToString();
                    string zz1 = float.Parse(_grass[i].DryColor[2].ToString()).ToString();
                    textWriter.WriteLine(xx1 + "," + yy1 + "," + zz1);
                    
                    textWriter.WriteLine(_grass[i].Billboard);
                    textWriter.WriteLine(_grass[i].Filemask);
                }
            }

            //<TerrainOption>
            textWriter.WriteLine("<TerrainOption>");
            textWriter.WriteLine(_imagesPath);
            textWriter.WriteLine(_heightmapMask);
            textWriter.WriteLine(_terrainMask);
            textWriter.WriteLine(_depthIndex);
            textWriter.WriteLine(_byteorderIndex);

            string x = float.Parse(_heightmapsize[0].ToString()).ToString();
            string y = float.Parse(_heightmapsize[1].ToString()).ToString();
            textWriter.WriteLine(x + "," + y);

            string x1 = float.Parse(_terrainsize[0].ToString()).ToString();
            string y1 = float.Parse(_terrainsize[1].ToString()).ToString();
            string z1 = float.Parse(_terrainsize[2].ToString()).ToString();
            textWriter.WriteLine(x1 + "," + y1 + "," + z1);


            textWriter.WriteLine(_heightmaperror);
            textWriter.WriteLine(_basemapdistance);
            textWriter.WriteLine(_castShadows);
            textWriter.WriteLine(AssetDatabase.GetAssetPath(_mat));
            textWriter.WriteLine(_detailObjectDistance);
            textWriter.WriteLine(_detailObjectDensity);
            textWriter.WriteLine(_treeDistance);
            textWriter.WriteLine(_treeBillboardDistance);
            textWriter.WriteLine(_treeCrossFadeLength);
            textWriter.WriteLine(_treeMaximumFullLodCount);

            textWriter.WriteLine(_windspeed);
            textWriter.WriteLine(_windsize);
            textWriter.WriteLine(_bendfactor);

            string x2 = float.Parse(_grasstint[0].ToString()).ToString();
            string y2 = float.Parse(_grasstint[1].ToString()).ToString();
            string z2 = float.Parse(_grasstint[2].ToString()).ToString();
            textWriter.WriteLine(x2 + "," + y2 + "," + z2);

            //Закроем запись
            textWriter.Close();
        }
    }

    private void AddNewSplatmap()
    {
        int i;
		
        i = _splats.Count + 1;
		SplatMapInfo2 sp = new SplatMapInfo2();
		sp.TileOffsetX = 0;
		sp.TileOffsetY = 0;
        sp.TileSizeX = 16;
        sp.TileSizeY = 16;
        sp.Texture = new Texture2D(1, 1);
        sp.Filemask = "ground" + i + "_x{0}_y{1}";
        sp.FoldOutStat = false;
        _splats.Add(sp);
		
		i = _normals.Count + 1;
        NormalMapInfo2 nm = new NormalMapInfo2();
        nm.TileOffsetX = sp.TileOffsetX;
		nm.TileOffsetY = sp.TileOffsetY;
        nm.TileSizeX = sp.TileSizeX;
        nm.TileSizeY = sp.TileSizeY;
        nm.Texture = new Texture2D(1, 1);
        nm.Filemask = "ground" + i + "_x{0}_y{1}";
        nm.FoldOutStat = false;
        _normals.Add(nm);
    }

    private void AddNewTreemap()
    {
        TreeMapInfo2 tm = new TreeMapInfo2();
        tm.Prefab = null;
        tm.BendFactor = 0;
        tm.ColorVariation = 0;
        tm.HeightScale = 100;
        tm.WidthScale = 100;
        tm.HeightVariation = 0;
        tm.WidthVariation = 0;
        tm.MaxDensity = 1;
        tm.Filemask = "tressmap_x{0}_y{1}";
        tm.FoldOutStat = false;
        _trees.Add(tm);
    }

    private void AddNewGrassmap()
    {
        GrassInfo2 gi = new GrassInfo2();
        gi.Texture = new Texture2D(1, 1);
        gi.Filemask = "grassmap_x{0}_y{1}";
        gi.MinWidth = 1;
        gi.MaxWidth = 2;
        gi.MinHeight = 1;
        gi.MaxHeight = 2;
        gi.NoiseSpread = 0.5f;
        gi.Billboard = true;
        gi.RenderMode = DetailRenderMode.GrassBillboard;
        gi.DryColor = new Color(0.698f, 0.6f, 0.5f);
        gi.HealthyColor = new Color(0.2627f, 0.9764f, 0.1647f);
        gi.FoldOutStat = false;
        gi.GrassDensity = 1;
        _grass.Add(gi);
    }

    private void DeleteSplatmap(int index)
    {
        _splats.RemoveAt(index);
		_normals.RemoveAt(index);
    }

    private void DeleteTreemap(int index)
    {
        _trees.RemoveAt(index);
    }

    private void DeleteGrassmap(int index)
    {
        _grass.RemoveAt(index);
    }

    private string VerifyImportData()
    {
        System.Text.StringBuilder strb = new System.Text.StringBuilder("");
        // Check heightmap information
        if (string.IsNullOrEmpty(_imagesPath))
        {
            strb.AppendLine("Plase inform the path where heightmap and splatmap image files can be found.");
            strb.AppendLine();
        }
        if (string.IsNullOrEmpty(_heightmapMask))
        {
            strb.AppendLine(
                "Plase inform the heightmap filename mask, with {0} for the X coordinate and {1} for the Y coordinate. Example: terrain_x{0}y{1}.raw");
            strb.AppendLine();
        }
        if (string.IsNullOrEmpty(_terrainMask))
        {
            strb.AppendLine(
                "Plase inform the name mask which you wish the terrain assets to be created, with {0} for the X coordinate and {1} for the Y coordinate. Example: terrain_x{0}y{1}.");
            strb.AppendLine();
        }

        if (_splats.Count > 0)
        {
            for (int i = 0; i < _splats.Count; i++)
            {
                SplatMapInfo2 sp = _splats[i];
                if (string.IsNullOrEmpty(sp.Filemask))
                {
                    strb.AppendLine("Plase inform the splatmap filename mask for splatmap " + (i + 1).ToString() +
                                    ", with {0} for the X coordinate and {1} for the Y coordinate. Example: grass_x{0}y{1}");
                    strb.AppendLine();
                }
            }
        }
		if (_normals.Count > 0)
        {
            for (int i = 0; i < _normals.Count; i++)
            {
                NormalMapInfo2 nm = _normals[i];
                if (string.IsNullOrEmpty(nm.Filemask))
                {
                    strb.AppendLine("Plase inform the splatmap filename mask for splatmap " + (i + 1).ToString() +
                                    ", with {0} for the X coordinate and {1} for the Y coordinate. Example: grass_x{0}y{1}");
                    strb.AppendLine();
                }
            }
        }

        if (_trees.Count > 0)
        {
            for (int i = 0; i < _trees.Count; i++)
            {
                TreeMapInfo2 tm = _trees[i];
                if (string.IsNullOrEmpty(tm.Filemask))
                {
                    strb.AppendLine("Plase inform the tree map filename mask for tree map " + (i + 1).ToString() +
                                    ", with {0} for the X coordinate and {1} for the Y coordinate. Example: grass_x{0}y{1}");
                    strb.AppendLine();
                }
            }
        }

        if (_grass.Count > 0)
        {
            for (int i = 0; i < _grass.Count; i++)
            {
                GrassInfo2 gm = _grass[i];
                if (string.IsNullOrEmpty(gm.Filemask))
                {
                    strb.AppendLine("Plase inform the grass map filename mask for grass map " + (i + 1).ToString() +
                                    ", with {0} for the X coordinate and {1} for the Y coordinate. Example: grass_x{0}y{1}");
                    strb.AppendLine();
                }
            }
        }
        return strb.ToString();
    }

    private void GetXYPosition(string hmfilemask, string hmfile, out string xvalue, out string yvalue)
    {
        string[] explodedstring = hmfilemask.Split("*".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
        string regex1 = explodedstring[0] + @"([0-9]+)" + explodedstring[1];
        System.Text.RegularExpressions.Regex r1 = new System.Text.RegularExpressions.Regex(regex1);
        System.Text.RegularExpressions.Match match1 = r1.Match(hmfile);
        xvalue = match1.Groups[1].Value;

        string regex2 = explodedstring[0] + xvalue + explodedstring[1] + @"([0-9]+)";
        System.Text.RegularExpressions.Regex r2 = new System.Text.RegularExpressions.Regex(regex2);
        System.Text.RegularExpressions.Match match2 = r2.Match(hmfile);
        yvalue = match2.Groups[1].Value;

    }

    private string ImportTerrain()
    {
        // Get all heightmap files
        string hmfilemask = _heightmapMask.Replace("{0}", "*").Replace("{1}", "*");
        string[] heightmapfiles = System.IO.Directory.GetFiles(_imagesPath, hmfilemask, System.IO.SearchOption.TopDirectoryOnly);
        string xvalue, yvalue;
        // Get Progress steps
        float files = heightmapfiles.Length;
        float progressstep = 1.0f / files;
        //Put Terrain in GameObject
        int x = 0, z = 0, len = -1;

        // Iterate through all heightmaps
        for (int i = 0; i < heightmapfiles.Length; i++)
        {
            //Создаем подпапку Terrain в папке Assets

            string f = Application.dataPath + "/";
            Directory.CreateDirectory(f + "Terrain");

            // Создаем новый TerrainData объект as we'll SetHeight it.
            TerrainData tdata = new TerrainData();
            tdata.heightmapResolution = (int)_heightmapsize.x; //Разрешения сетки(меша) или размер подгружаемого файла
            tdata.size = _terrainsize; // размер карты xyz 
            tdata.alphamapResolution = (int)_heightmapsize.x; //Control Texture Resolution разрешение текстур наносимых на ландшафт. 
			//В Unity известна как Splatmap текстура, значение Control Texture Resolution (разрешение контрольной текстуры) управляет размером, 
			//а, следовательно, и детализацией, любой текстуры наносимой на ландшафт. Высокое разрешение сильно влияет на производительность, 
			//поэтому стало хорошей практикой, оставлять это значение равным 512;
            tdata.baseMapResolution = 512;//(int)_heightmapsize.x; // разрешение текстуры используемой Unity для визуализации(render) рельефа 
			//ландшафта на расстоянии, при значительном отдалении игровой камеры или на слабых компьютерах
			//tdata.detailResolution = 512; //Чем больше значение, тем более точно вы можете разместить 
			//камни, трава, кусты и т.п. на ландшафте, с точки зрения позиционирования
			

            GetXYPosition(hmfilemask, heightmapfiles[i], out xvalue, out yvalue); //название сохраняемого asset-a

            string assetname = string.Format(_terrainMask, xvalue, yvalue);
            AssetDatabase.CreateAsset(tdata, "Assets/Terrain/" + assetname + ".asset");
            AssetDatabase.SaveAssets();
            TerrainData td = (TerrainData)AssetDatabase.LoadAssetAtPath("Assets/Terrain/" + assetname + ".asset", typeof(TerrainData));
            
            td.wavingGrassAmount = _bendfactor; //Количество размахивая траву в местности
            td.wavingGrassSpeed = _windsize ; //скорость размахивание травы
            td.wavingGrassStrength = _windspeed; // сила раскачивания
            td.wavingGrassTint = _grasstint; // цвет травы
            
            // Получим data в файл террайна
            // Get the data from the file
            ParseFile(heightmapfiles[i], ref td);
            
            // Подгрузим spaltsmapы в файл террайна
            if (_splats.Count > 0)
            {
                LoadSplats(xvalue, yvalue, ref td);
            }
            // Подгрузим деревья
            if (_trees.Count > 0)
            {
                LoadTrees(xvalue, yvalue, td);
            }

            DetailPrototype[] grassprototypes = null;
            int[][,] grassarray = null;
            // Подгрузим траву
            LoadGrass(xvalue, yvalue, td, ref grassprototypes, ref grassarray);
            
            if (_grass.Count > 0)
            {
                td.SetDetailResolution((int)_heightmapsize.x, 16);
                td.detailPrototypes = grassprototypes;
                for (int j = 0; j < grassarray.Length; j++)
                {
                    td.SetDetailLayer(0, 0, j, grassarray[j]);
                }
            }

            td.RefreshPrototypes();
            
            AssetDatabase.SaveAssets(); // Запись Assets terrain

             //Создание и размещение GameObject в игровой сцене из tdata
            if (_generateTerrain)
            {
                GameObject map = Terrain.CreateTerrainGameObject(tdata); //Создадим игровой обект из даты
                map.name = tdata.name; //Обзовем карту
				//Добавляем к террейну скрипт TerrainShader.cs
				map.AddComponent("TerrainShader");

                len++;
                if (len < Math.Sqrt(heightmapfiles.Length)) //Если количество проходов меньше квадратного корня тогда
                {

                    map.transform.position = new Vector3(tdata.size.x*x, 0, tdata.size.x*z);
                        // запишим новые координаты карты
                    z++;
                    TerrainOption(); //применим к терайну всякие фичи
                }
                else
                {
                    x++;
                    len = 0;
                    z = 0;
                    map.transform.position = new Vector3(tdata.size.x*x, 0, tdata.size.x*z);
                    z++;
                    TerrainOption(); //применим к терайну всякие фичи
                }
                
            }
            //

            if (_generateBundle)
            {
                BuildPipeline.BuildAssetBundle(td, new UnityEngine.Object[1] { td }, "Assets/Terrain/" + assetname + ".unity3d");
                AssetDatabase.DeleteAsset("Assets/Terrain/" + assetname + ".asset");
            }

          
            System.GC.Collect(0);

            _progress += progressstep;
            EditorUtility.DisplayProgressBar("Импорт террейнов", ((int)(_progress * 100)).ToString() + "% осталось...", _progress);
            UnityEditor.EditorUtility.UnloadUnusedAssetsIgnoreManagedReferences();
        }
       
       AssetDatabase.Refresh();
        _progress = 0;
    
        EditorUtility.ClearProgressBar();
        System.GC.Collect(0);
        switch (_errorString)
        {
            case "Все прошло успешно.":
                return "Все прошло успешно.";

            case "Данное число не входит в диапазон":
                return "Данное число не входит в диапазон";
            
            case "Делить на ноль нельзя":
                return "Делить на ноль нельзя";

            case "Индекс выходит за пределы.":
                return "Индекс выходит за пределы.";

        }
        return "Script Finish :)";
    }
    
    private void TerrainOption()//применим к терайну всякие фичи
    {
        Terrain.activeTerrain.basemapDistance = _basemapdistance; //Качество текстуры чем выше тем лучше
        Terrain.activeTerrain.heightmapPixelError = _heightmaperror; // Качество сетки 1 самое крутое
        Terrain.activeTerrain.castShadows = _castShadows; // Тени на карте
        Terrain.activeTerrain.materialTemplate = _mat; //подгрузим материал
        Terrain.activeTerrain.detailObjectDistance = _detailObjectDistance; // С какой дистанции рисовать обекты
        Terrain.activeTerrain.detailObjectDensity = _detailObjectDensity; // изщменияеться с 0.1 до 1 вроде как тоже отвечает за детали а так хер его знает че он делает
        Terrain.activeTerrain.treeDistance = _treeDistance;
        Terrain.activeTerrain.treeBillboardDistance = _treeBillboardDistance;
        Terrain.activeTerrain.treeCrossFadeLength = _treeCrossFadeLength;
        Terrain.activeTerrain.treeMaximumFullLODCount = _treeMaximumFullLodCount;
		
    }

    private void ParseFile(string heightmapfile, ref TerrainData tdata)
    {
        // Okay, now that we got the x and y coordinate for this terrain, load the terrain for read
        byte[] data = System.IO.File.ReadAllBytes(heightmapfile);
        // Create a heightmapdata array
        float[,] heightmapData = new float[(int) _heightmapsize.x,(int) _heightmapsize.y];
        int counterx = 0;
        int countery = 0;

        try // Обработка ошибок
        {

            for (long k = 0; k < data.Length; k++)
            {

                int val = (int) data[k];
                float hmval = 0;
                if (_depthIndex == 1)
                {
                    k++;
                    int val2 = (int) data[k];
                    ushort hmvaltemp;
                    if (_byteorderIndex == 0)
                        hmvaltemp = (ushort) (val + (val2 << 8));
                    else
                        hmvaltemp = (ushort) ((val << 8) + val2);
                    hmval = (float) hmvaltemp;
                }
                else
                {
                    hmval = (float) val;
                }
                float normalized = 0;
                if (_depthIndex == 1)
                    normalized = (float) hmval/65535;
                else
                    normalized = (float) hmval/255;

                heightmapData[countery, counterx] = normalized;
                counterx++;
                if (counterx >= (int) _heightmapsize.x)
                {
                    counterx = 0;
                    countery++;
                }
            }
            data = null;
            tdata.SetHeights(0, 0, heightmapData);
            heightmapData = null;
            System.GC.Collect(0);
        }
        // Обрабатываем исключение, возникающее
        // при арифметическом переполнении
        catch (OverflowException)
        {
            //Debug.Log("OverflowException"); // Данное число не входит в какой то диапазон 
            _errorString = "Данное число не входит в диапазон";
            Debug.LogError(_errorString);
        }
        // Исключение при делении на 0
        catch (DivideByZeroException)
        {
            _errorString = "Делить на ноль нельзя";
            Debug.LogError("DivideByZeroException");
        }
        // Исключение при переполнении массива
        catch (IndexOutOfRangeException)
        {
            _errorString = "Индекс выходит за пределы.";
            Debug.LogError (_errorString + " Несоответствует размер Hieghtmap Size: " + _heightmapsize + "Размеру входящего файла");
            }
    }
    // Нормализуем слои в масках текстур
    private void NORMALIZ(ref float[, ,] splatarray)
    {
                /*
                 *  Идея alexz
                 *  http://www.unity3d.ru/distribution/viewtopic.php?f=19&t=3652&st=0&sk=t&sd=a&start=30
                 *  - А у меня друг сломал сервер за пять минут!!!
                 *  - Он что, хакер???
                 *  - Нет. Он - мудак!!!
                 */

                 // Нормализум то что не нормализованно
                 // Нормализация (normalization) — приведение к единичному размеру.
                 // Термин «Normalization» также переводится как «Нормировка», смысл которой сосотоит в приведении чего-либо к эталонному,
                 // единичному виду. Например, отнормировать последовательность означает умножит ее члены на такую величину,
                 // чтобы максимум (по модулю) этой последовательности стал равен единице.
        
       
            for (int y = 0; y < _heightmapsize.y; y++)
            {
                for (int x = 0; x < _heightmapsize.x; x++)
                {
                    float tsum = 0; //храним сумму слоев
                   // float bsum = 0; //храним сумму слоев отсечения

                    float[] sum = new float[_splats.Count]; //Храним хронику сложений слоев
                   // int layermem = 0; //Запишим номер слоя отсечения
                    //Посчитаем сумму слоев
                    for (int layer = 0; layer < _splats.Count; layer++)
                    {
                        tsum += splatarray[x, y, layer]; // запишим сумму весов всех слоев
                        sum[layer] += splatarray[x, y, layer]; //число фибаначи
                    }
                    /* НЕДОДЕЛАННО
                       // Найдем последний не нулевой слой
                       if (tsum != 0) // Если сумма значений в слоях не равняеться 0
                       {
                           layermem = _splats.Count;
                           float j = 0f;
                           do
                           {
                               layermem--; // Номер слоя в котором есть значения
                               j = splatarray[x, y, layermem];
                            
                           } while (j == 0);


                           //  Посчитаем сумму слоев которые нужно смешть по отношению к главному слою
                           if (layermem == 0 ) //Если есть хотя бы один слой
                           { 
                               for (int layer = 0; layer < layermem; layer++)
                               {
                                   bsum += splatarray[x, y, layer]; // запишим сумму весов слоев смешивания
                               }

                               for (int layer = 0; layer < layermem; layer++)
                               {
                                   if (bsum == 0)
                                   {
                                       splatarray[x, y, layer] *= _ratioleft / splatarray[x, y, layer]; // запишим значения слоя с учетом коэффициента (_ratio)
                                   }
                                   else
                                   {
                                       splatarray[x, y, layer] *= _ratioleft / bsum; // запишим значения слоя с учетом коэффициента (_ratio)
                                   }
                               }

                               splatarray[x, y, layermem ] *= _ratioright / splatarray[x, y, layermem]; // Запишим послейдний слой

                           }
                       }

                       // Debug.LogError(string.Format("Запись внутри цыкла. Сумма слоев = 0, коэффициент: ({0}) всего слоев: ({1}) сумма весов слоев: ({2}) № слоя ({3}), вес слоя ({4})",
                       // 1/tsum, _splats.Count, tsum, layer, splatarray[x, y, layer]));
                    
                    //Нормализуем
                    for (int layer = 0; layer < _splats.Count; layer++)
                    {
                        tsum += splatarray[x, y, layer]; // запишим сумму весов всех слоев
                        sum[layer] += splatarray[x, y, layer]; //число фибаначи
                    }
                     НЕДОДЕЛАНО      
                     */
                    if (tsum > 1)
                    {
                        for (int layer = 0; layer < _splats.Count; layer++)
                        {
                          splatarray[x, y, layer] *= 1/tsum;
                        }
                    }
                }
            }
    }
        
    //конец блока нормализации
    

    //Применяем полученый SplatMap на терейн дату 
    private void LoadSplats(string xvalue, string yvalue, ref TerrainData tdata)
    {

        if (_splats.Count > 0)
        {
            float[, ,] splatarray = new float[(int)_heightmapsize.x, (int)_heightmapsize.y, _splats.Count];
            SplatPrototype[] prototypes = new SplatPrototype[_splats.Count];

            for (int k = 0; k < _splats.Count; k++)
            {
                LoadSplat(_splats[k], _normals[k], k, xvalue, yvalue, ref prototypes, ref splatarray);
                // Прогресс бар (тормазим работу :)
                //EditorUtility.DisplayProgressBar("Importing Splat", ((k + 1)).ToString() + " Splat Files", 1);
                
            }
            if (_optionSplatMix) //Если стоит галочка нормализовать
            {
                NORMALIZ(ref splatarray);
            }

            tdata.splatPrototypes = prototypes;
            tdata.SetAlphamaps(0, 0, splatarray);
            prototypes = null;
            splatarray = null;
            System.GC.Collect(0);

        }
    }

    //Загружаем каждый слой отдельно
    private void LoadSplat(SplatMapInfo2 splat, NormalMapInfo2 normal, int k, string xvalue, string yvalue, ref SplatPrototype[] prototypes, ref float[,,] splatarray)
    {

       // Load splatmap file
        string splatfile = string.Format(splat.Filemask, xvalue, yvalue); //Get Name Files, x and y size
        string[] splatfiles = System.IO.Directory.GetFiles(_imagesPath, splatfile + ".*"); // Найдем файл
        byte[] imagebytes = System.IO.File.ReadAllBytes(splatfiles[0]); // чтение из файла 
        Texture2D t = new Texture2D(1, 1); // создадим пустой обект t
        t.LoadImage(imagebytes); // загрузим в t днные с слоя
        imagebytes = null; //чистим память
        System.GC.Collect(0);// чистим
        Color[] imagecolors = t.GetPixels();// передадми дату в масив imagecolors
        int imagewidth = t.width;
        
        DestroyImmediate(t); // укакошим t
        
        for (int v = 0; v < _heightmapsize.y; v++) // будем крутить пока Y  _heightmapsize  не закончится
        {
            for (int u = 0; u < _heightmapsize.x; u++) // крутим _heightmapsize по X
            {
                splatarray[u, v, k] = imagecolors[((imagewidth - 1) - u)*imagewidth + v].r; // загрузим 
            }
        }

        imagecolors = null;
        System.GC.Collect(0);
        // Set splat prototype for this splat layer
        SplatPrototype prototype = new SplatPrototype();
        prototype.texture = splat.Texture;
		prototype.normalMap  = normal.Texture;
        prototype.tileOffset = new Vector2(splat.TileOffsetX, splat.TileOffsetY);
        prototype.tileSize = new Vector2(splat.TileSizeX, splat.TileSizeY);
        prototypes[k] = prototype;

        }



    private void LoadTrees(string xvalue, string yvalue, TerrainData tdata)
    {
        if (_trees.Count > 0)
        {
            System.Collections.Generic.List<TreeInstance> treelist = new System.Collections.Generic.List<TreeInstance>();
            TreePrototype[] prototypes = new TreePrototype[_trees.Count];

            for (int k = 0; k < _trees.Count; k++)
            {
                TreeMapInfo2 tree = _trees[k];
                // Load splatmap file
                string treefile = string.Format(tree.Filemask, xvalue, yvalue);
                string[] treefiles = System.IO.Directory.GetFiles(_imagesPath, treefile + ".*");
                System.Uri uri = new System.Uri(treefiles[0]);
                WWW imageloader = new WWW(uri.AbsoluteUri);
                treefiles = null;

                while (!imageloader.isDone)
                {
                }

                Color[] imagecolors = imageloader.texture.GetPixels();
                int imagewidth = imageloader.texture.width;
                imageloader.Dispose();

                // Create Tree Prototype
                TreePrototype tp = new TreePrototype();
                tp.prefab = tree.Prefab;
                tp.bendFactor = tree.BendFactor;
                prototypes[k] = tp;

                float treefactor = 0;

                for (int u = 0; u < _heightmapsize.x; u++)
                {
                    for (int v = 0; v < _heightmapsize.y; v++)
                    {
                        //if (imagecolors[(((imagewidth - 1) - v) * imagewidth) + u].grayscale >= 0.035)
                        //{
                        // Get treemap resolution / terrain size bias
                        float xbias = _terrainsize.x/imagewidth;
                        float ybias = _terrainsize.z/imagewidth;

                        // Calculate Tree Position
                        float terrainheight = tdata.size.y;
                        float terrainwidth = tdata.size.x;

                        Vector3 treeposition = new Vector3();
                        treeposition.x = (v*xbias)/terrainwidth;
                        treeposition.y = tdata.GetHeight(v, u)/terrainheight;
                        treeposition.z = (u*ybias)/terrainwidth;

                        TreeInstance ti = new TreeInstance();
                        ti.prototypeIndex = k;
                        ti.position = treeposition;
                        ti.lightmapColor = new Color(1, 1, 1);
                        ti.heightScale = (tree.HeightScale/100) +
                                         UnityEngine.Random.Range(-(tree.HeightVariation/100),
                                                                  (tree.HeightVariation/100));
                        ti.widthScale = (tree.WidthScale/100) +
                                        UnityEngine.Random.Range(-(tree.WidthVariation/100), (tree.WidthVariation/100));
                        ti.color = new Color(1, 1, 1);

                        float treestoplace = imagecolors[((imagewidth - 1) - u)*imagewidth + v].r*tree.MaxDensity;
                        treefactor += treestoplace;
                        if (treefactor > (1.0f + UnityEngine.Random.Range(-0.2f, 0.2f)))
                        {
                            treelist.Add(ti);
                            treefactor = 0;
                        }
                    }
                }
            }
            tdata.treePrototypes = prototypes;
            tdata.treeInstances = treelist.ToArray();
            prototypes = null;
            treelist.RemoveRange(0, treelist.Count);
            treelist = null;
            System.GC.Collect(0);

        }
    }

    private void LoadGrass(string xvalue, string yvalue, TerrainData tdata, ref DetailPrototype[] prototypes,
                           ref int[][,] grassmap)
    {
        if (_grass.Count > 0)
        {
            grassmap = new int[_grass.Count][,];
            prototypes = new DetailPrototype[_grass.Count];
            for (int i = 0; i < _grass.Count; i++)
            {
                GrassInfo2 gr = _grass[i];
                // Create grass prototype
                // Load splatmap file
                string grassfile = string.Format(gr.Filemask, xvalue, yvalue);
                string[] grassfiles = System.IO.Directory.GetFiles(_imagesPath, grassfile + ".*");
                System.Uri uri = new System.Uri(grassfiles[0]);
                WWW imageloader = new WWW(uri.AbsoluteUri);
                grassfiles = null;
                while (!imageloader.isDone)
                {
                }

                // Parse splat image and populate array
                Color[] imagecolors = imageloader.texture.GetPixels();
                int imagewidth = imageloader.texture.width;
                imageloader.Dispose();

                grassmap[i] = new int[(int) _heightmapsize.x,(int) _heightmapsize.x];

                for (int v = 0; v < _heightmapsize.y; v++)
                {
                    for (int u = 0; u < _heightmapsize.x; u++)
                    {
                        grassmap[i][u, v] = (int) ((float) imagecolors[((imagewidth - 1) - u)*imagewidth + v].r*(10*gr.GrassDensity));
                    }
                }
                DetailPrototype prototype = new DetailPrototype();
                prototype.prototypeTexture = gr.Texture;
                prototype.minWidth = gr.MinWidth;
                prototype.maxWidth = gr.MaxWidth;
                prototype.minHeight = gr.MinHeight;
                prototype.maxHeight = gr.MaxHeight;
                prototype.noiseSpread = gr.NoiseSpread;
                prototype.usePrototypeMesh = false;
                prototype.bendFactor = _bendfactor;
                prototype.dryColor = gr.DryColor;
                prototype.healthyColor = gr.HealthyColor;
                if (gr.Billboard) prototype.renderMode = DetailRenderMode.GrassBillboard;
                else prototype.renderMode = DetailRenderMode.Grass;
                prototypes[i] = prototype;
            }
        }
    }
    
}


public struct SplatMapInfo2
{
    public Texture2D Texture;
    public int TileSizeX;
    public int TileSizeY;
    public int TileOffsetX;
    public int TileOffsetY;
    public string Filemask;
    public bool FoldOutStat;
}

public struct NormalMapInfo2
{
    public Texture2D Texture;
    public int TileSizeX;
    public int TileSizeY;
    public int TileOffsetX;
    public int TileOffsetY;
    public string Filemask;
    public bool FoldOutStat;
}

public struct TreeMapInfo2
{
    public GameObject Prefab;
    public int BendFactor;
    public float ColorVariation;
    public int HeightScale;
    public int WidthScale;
    public int HeightVariation;
    public int WidthVariation;
    public float MaxDensity;
    public string Filemask;
    public bool FoldOutStat;
}

public struct GrassInfo2
{
    public Texture2D Texture;
    public float MinWidth;
    public float MaxWidth;
    public float MinHeight;
    public float MaxHeight;
    public float NoiseSpread;
    public float GrassDensity;
    public Color HealthyColor;
    public Color DryColor;
    public DetailRenderMode RenderMode;
    public bool Billboard;
    public string Filemask;
    public bool FoldOutStat;
}