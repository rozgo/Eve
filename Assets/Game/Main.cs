using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Main : MonoBehaviour {

    IEnumerator Start () {
        Instantiator.Instantiate( "Common/SharedShaders", "Environment", null, null );
        Instantiator.Instantiate( "HomeScene/Environment", "Environment", null, null );
        yield return StartCoroutine( Definitions.Get().LoadDefinitions() );
        yield return StartCoroutine( LoadBlocks( "http://dl.dropboxusercontent.com/u/10592653/eve/UnitTestBlocks.json" ) );
    }

	public IEnumerator LoadBlocks ( string url ) {
		yield return StartCoroutine( Requestor.ForJSON( url, ( json ) => {
			var obj = MiniJSON.Json.Deserialize( json );
			Dynamic.For<Dictionary<string,object>>( obj, blockTypes => {
				var createdBlocks = new List<Blocks.Block>();
				foreach ( var blockType in blockTypes.Keys ) {
					Dynamic.For<Dictionary<string,object>>( blockTypes[blockType], blocks => {
						foreach ( var blockId in blocks.Keys ) {
							Dynamic.For<Dictionary<string,object>>( blocks[blockId], record => {
								var entityId = record["entity"] as string;
								var entity = GameObject.Find( entityId ) as GameObject;
								if ( entity == null ) {
									entity = new GameObject( entityId );
								}
								var block = entity.AddComponent(blockType as string) as Blocks.Block;
								createdBlocks.Add( block );
								block.OnDataBind();
								block.OnSource( blockId as string, record );
							} );
						}
					} );
				}
				foreach ( var createdBlock in createdBlocks ) {
					createdBlock.OnSetup();
				}
			} );
		} ) );
	}

}
