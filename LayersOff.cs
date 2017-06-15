using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class LayersOff : MonoBehaviour {

	//////////////////////////////////////
	//Script by PerguGames
	//GitHub:https://github.com/PerduGames
	//////////////////////////////////////

	/*
	Script para encontrar as layers não utilizadas na cena 
	ou em todo projeto, recebendo seus nomes no Console ou
	criando arquivo .txt com as layers não utilizadas.

	Notes:
	- Para atualizar o arquivo .txt criado na Unity, minimize a Unity ou de um Refresh que atualiza.
	- Ouvi dizer que este por ser um recurso interno pode quebrar em futuras versões:
	"UnityEditorInternal.InternalEditorUtility.layers"
	-No modo pesquisa para pesquisar somente na cena(true), apenas pesquisa objetos ativos, caso esteja
	usando uma versão antiga da Unity, apenas mude onde houver "FindObjectsOfType" para "FindObjectsOfTypeAll",
	assim também pesquisará em objetos inativos na cena.
	*/

	//Bool para pesquisar dentro da cena ou do projeto inteiro
	[Header("Modo Pesquisa?")]
	[Tooltip("True = Pesquisar somente na cena. \nFalse = Pesquisar em todo projeto.")]
	[SerializeField]
	private bool ModoPesquisa;

	//Bool para gerar arquivo txt ou não
	[Header("Gerar arquivo .txt?")]
	[Tooltip("True = Gerar arquivo .txt. \nFalse = Não gerar arquivo .txt.")]
	[SerializeField]
	private bool GerarArquivo;

	//Bool para evitar layers padrão da unity
	[Header("Evitar Layers padrões da Unity?")]
	[Tooltip("True = Evitar Layers padrões da Unity. \nFalse = Não evitar Layers padrões da Unity.")]
	[SerializeField]
	private bool EvitarLayersUnity;

	//Lista para guardar layers não utilizadas
	private List<string> listaLayers = new List<string>();
	//Criar array de layers padrão da Unity
	string[] layersUnity = {"Default", "TransparentFX", "Ignore Raycast", "Water", "UI", ""};

	// Use this for initialization
	void Awake () {
		//Pesquisar somente na cena
		if (ModoPesquisa == true) {
			//Colocar as layers na lista
			listaLayers = UnityEditorInternal.InternalEditorUtility.layers.ToList ();

			for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.layers.Length; i++) {
				int num = 0;
				//Verificar se a layer está sendo utilizada por algum objeto
				Object[] objs = GameObject.FindObjectsOfType (typeof(GameObject));
				for (int j = 0; j < objs.Length; j++) {
					GameObject o = (GameObject)objs[j];
					if(o.layer == LayerMask.NameToLayer(UnityEditorInternal.InternalEditorUtility.layers [i])){
						num += 1;
					}
					//Pesquisar nos objetos filhos caso haja
					if (o.transform.childCount > 0) {
						for (int k = 0; k < o.transform.childCount; k++) {
							if (o.transform.GetChild (k).gameObject.layer == LayerMask.NameToLayer(UnityEditorInternal.InternalEditorUtility.layers [i])) {
								num += 1;
							}
						}
					}
				}
				//Se a layers estiver sendo utilizada, remove ela da lista
				if(num != 0){
					listaLayers.Remove (UnityEditorInternal.InternalEditorUtility.layers [i]);
				}
			}
			//Evitar as layers e gerar arquivo
			gerar ();
		//Pesquisar em todo projeto
		}else if(ModoPesquisa == false){
			//Criar lista de GameObject para os prefabs
			List<GameObject> listaPrefabs = new List<GameObject>();
			//Pegar FileInfo de todos prefabs do projeto
			DirectoryInfo dir = new DirectoryInfo ("Assets/");
			FileInfo[] arquivo = dir.GetFiles ("*.prefab", System.IO.SearchOption.AllDirectories);
			//Pegar todos os GameObjects 
			foreach(FileInfo i in arquivo){
				string pathCompleto = i.FullName.Replace(@"\","/");
				string pathUnity = "Assets" + pathCompleto.Replace(Application.dataPath, "");
				GameObject gobj = AssetDatabase.LoadAssetAtPath (pathUnity, typeof(GameObject)) as GameObject;
				listaPrefabs.Add (gobj);
			}

			//Colocar as layers na lista
			listaLayers = UnityEditorInternal.InternalEditorUtility.layers.ToList ();

			//Pecorrer a lista das layers
			for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.layers.Length; i++) {
				int num = 0;
				//Pecorrer a lista dos objetos
				for (int j = 0; j < listaPrefabs.Count; j++) {
					//Pesquisar no objeto pai
					if (listaPrefabs [j].layer == LayerMask.NameToLayer(UnityEditorInternal.InternalEditorUtility.layers [i])) {
						num += 1;
					}
					//Pesquisar nos objetos filhos caso haja
					if (listaPrefabs [j].transform.childCount > 0) {
						for (int k = 0; k < listaPrefabs [j].transform.childCount; k++) {
							if(listaPrefabs [j].transform.GetChild(k).gameObject.layer == LayerMask.NameToLayer(UnityEditorInternal.InternalEditorUtility.layers [i])){
								num += 1;
							}
						}
					}
				}
				//Se a layers estiver sendo utilizada, remove ela da lista
				if(num != 0){
					listaLayers.Remove (UnityEditorInternal.InternalEditorUtility.layers [i]);
				}
			}
			//Evitar as layers e gerar arquivo
			gerar ();
		}
	}

	//Metodo para evitar as layers e gerar arquivo caso seja true
	void gerar (){
		//Para evitar layers padrão da Unity
		if(EvitarLayersUnity == true){
			for(int i = 0; i < layersUnity.Length; i++) {
				listaLayers.Remove (layersUnity [i]);
			}
		}
		//String para escrever no arquivo .txt
		string stg = "";
		//Mostrar as layers não utilizadas no Console
		for (int i = 0; i < listaLayers.Count; i++) {
			Debug.Log (listaLayers [i]);
			//Se GerarArquivo for true
			if(GerarArquivo == true){
				stg += listaLayers [i] + System.Environment.NewLine;
			}
		}
		//Se GerarArquivo for true
		if (GerarArquivo == true) {
			StreamWriter writer = new StreamWriter("Assets/LayersOff.txt");
			writer.WriteLine (stg);
			writer.Close();
		}
	}
}