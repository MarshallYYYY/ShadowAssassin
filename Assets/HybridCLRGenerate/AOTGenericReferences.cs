using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"DOTween.dll",
		"Newtonsoft.Json.dll",
		"Unity.InputSystem.dll",
		"UnityEngine.CoreModule.dll",
		"YooAsset.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// System.Action<System.ValueTuple<object,object,object>>
	// System.Action<UnityEngine.InputSystem.InputAction.CallbackContext>
	// System.Action<int>
	// System.Action<object>
	// System.Collections.Generic.ArraySortHelper<System.ValueTuple<object,object,object>>
	// System.Collections.Generic.ArraySortHelper<object>
	// System.Collections.Generic.Comparer<System.ValueTuple<object,object,object>>
	// System.Collections.Generic.Comparer<object>
	// System.Collections.Generic.Dictionary.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection<int,int>
	// System.Collections.Generic.Dictionary.KeyCollection<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection<int,int>
	// System.Collections.Generic.Dictionary.ValueCollection<object,object>
	// System.Collections.Generic.Dictionary<int,int>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.EqualityComparer<int>
	// System.Collections.Generic.EqualityComparer<object>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ICollection<System.ValueTuple<object,object,object>>
	// System.Collections.Generic.ICollection<object>
	// System.Collections.Generic.IComparer<System.ValueTuple<object,object,object>>
	// System.Collections.Generic.IComparer<object>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerable<System.ValueTuple<object,object,object>>
	// System.Collections.Generic.IEnumerable<object>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerator<System.ValueTuple<object,object,object>>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.IEqualityComparer<int>
	// System.Collections.Generic.IEqualityComparer<object>
	// System.Collections.Generic.IList<System.ValueTuple<object,object,object>>
	// System.Collections.Generic.IList<object>
	// System.Collections.Generic.KeyValuePair<int,int>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.List.Enumerator<System.ValueTuple<object,object,object>>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List<System.ValueTuple<object,object,object>>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<object,object,object>>
	// System.Collections.Generic.ObjectComparer<object>
	// System.Collections.Generic.ObjectEqualityComparer<int>
	// System.Collections.Generic.ObjectEqualityComparer<object>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.ValueTuple<object,object,object>>
	// System.Collections.ObjectModel.ReadOnlyCollection<object>
	// System.Comparison<System.ValueTuple<object,object,object>>
	// System.Comparison<object>
	// System.Predicate<System.ValueTuple<object,object,object>>
	// System.Predicate<UnityEngine.InputSystem.InputControlScheme>
	// System.Predicate<object>
	// System.ValueTuple<object,object,object>
	// UnityEngine.InputSystem.InputBindingComposite<UnityEngine.Vector2>
	// UnityEngine.InputSystem.InputControl<UnityEngine.Vector2>
	// UnityEngine.InputSystem.InputProcessor<UnityEngine.Vector2>
	// UnityEngine.InputSystem.Utilities.InlinedArray<object>
	// UnityEngine.InputSystem.Utilities.ReadOnlyArray.Enumerator<UnityEngine.InputSystem.InputControlScheme>
	// UnityEngine.InputSystem.Utilities.ReadOnlyArray<UnityEngine.InputSystem.InputControlScheme>
	// }}

	public void RefMethods()
	{
		// object DG.Tweening.TweenSettingsExtensions.OnComplete<object>(object,DG.Tweening.TweenCallback)
		// object DG.Tweening.TweenSettingsExtensions.SetLoops<object>(object,int,DG.Tweening.LoopType)
		// object Newtonsoft.Json.JsonConvert.DeserializeObject<object>(string)
		// object Newtonsoft.Json.JsonConvert.DeserializeObject<object>(string,Newtonsoft.Json.JsonSerializerSettings)
		// byte[] System.Array.Empty<byte>()
		// object& System.Runtime.CompilerServices.Unsafe.As<object,object>(object&)
		// System.Void* System.Runtime.CompilerServices.Unsafe.AsPointer<object>(object&)
		// System.Void* Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AddressOf<UnityEngine.Vector2>(UnityEngine.Vector2&)
		// int Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<UnityEngine.Vector2>()
		// object UnityEngine.Component.GetComponent<object>()
		// object[] UnityEngine.Component.GetComponentsInChildren<object>()
		// object[] UnityEngine.Component.GetComponentsInChildren<object>(bool)
		// object UnityEngine.GameObject.AddComponent<object>()
		// object UnityEngine.GameObject.GetComponent<object>()
		// object[] UnityEngine.GameObject.GetComponentsInChildren<object>(bool)
		// UnityEngine.Vector2 UnityEngine.InputSystem.InputAction.CallbackContext.ReadValue<UnityEngine.Vector2>()
		// UnityEngine.Vector2 UnityEngine.InputSystem.InputActionState.ApplyProcessors<UnityEngine.Vector2>(int,UnityEngine.Vector2,UnityEngine.InputSystem.InputControl<UnityEngine.Vector2>)
		// UnityEngine.Vector2 UnityEngine.InputSystem.InputActionState.ReadValue<UnityEngine.Vector2>(int,int,bool)
		// object UnityEngine.Object.FindObjectOfType<object>()
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform)
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform,bool)
		// YooAsset.AssetHandle YooAsset.ResourcePackage.LoadAssetAsync<object>(string,uint)
	}
}