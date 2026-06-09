using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class CompanionInputActions : IInputActionCollection2, IInputActionCollection, IEnumerable<InputAction>, IEnumerable, IDisposable
{
	public struct CompanionActions
	{
		private CompanionInputActions m_Wrapper;

		public InputAction Move => m_Wrapper.m_Companion_Move;

		public InputAction Jump => m_Wrapper.m_Companion_Jump;

		public InputAction Sprint => m_Wrapper.m_Companion_Sprint;

		public InputAction Meow => m_Wrapper.m_Companion_Meow;

		public InputAction Fire => m_Wrapper.m_Companion_Fire;

		public InputAction Interact => m_Wrapper.m_Companion_Interact;

		public bool enabled => Get().enabled;

		public CompanionActions(CompanionInputActions wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_Companion;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(CompanionActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(ICompanionActions instance)
		{
			if (instance != null && !m_Wrapper.m_CompanionActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_CompanionActionsCallbackInterfaces.Add(instance);
				Move.started += instance.OnMove;
				Move.performed += instance.OnMove;
				Move.canceled += instance.OnMove;
				Jump.started += instance.OnJump;
				Jump.performed += instance.OnJump;
				Jump.canceled += instance.OnJump;
				Sprint.started += instance.OnSprint;
				Sprint.performed += instance.OnSprint;
				Sprint.canceled += instance.OnSprint;
				Meow.started += instance.OnMeow;
				Meow.performed += instance.OnMeow;
				Meow.canceled += instance.OnMeow;
				Fire.started += instance.OnFire;
				Fire.performed += instance.OnFire;
				Fire.canceled += instance.OnFire;
				Interact.started += instance.OnInteract;
				Interact.performed += instance.OnInteract;
				Interact.canceled += instance.OnInteract;
			}
		}

		private void UnregisterCallbacks(ICompanionActions instance)
		{
			Move.started -= instance.OnMove;
			Move.performed -= instance.OnMove;
			Move.canceled -= instance.OnMove;
			Jump.started -= instance.OnJump;
			Jump.performed -= instance.OnJump;
			Jump.canceled -= instance.OnJump;
			Sprint.started -= instance.OnSprint;
			Sprint.performed -= instance.OnSprint;
			Sprint.canceled -= instance.OnSprint;
			Meow.started -= instance.OnMeow;
			Meow.performed -= instance.OnMeow;
			Meow.canceled -= instance.OnMeow;
			Fire.started -= instance.OnFire;
			Fire.performed -= instance.OnFire;
			Fire.canceled -= instance.OnFire;
			Interact.started -= instance.OnInteract;
			Interact.performed -= instance.OnInteract;
			Interact.canceled -= instance.OnInteract;
		}

		public void RemoveCallbacks(ICompanionActions instance)
		{
			if (m_Wrapper.m_CompanionActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(ICompanionActions instance)
		{
			foreach (ICompanionActions companionActionsCallbackInterface in m_Wrapper.m_CompanionActionsCallbackInterfaces)
			{
				UnregisterCallbacks(companionActionsCallbackInterface);
			}
			m_Wrapper.m_CompanionActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public interface ICompanionActions
	{
		void OnMove(InputAction.CallbackContext context);

		void OnJump(InputAction.CallbackContext context);

		void OnSprint(InputAction.CallbackContext context);

		void OnMeow(InputAction.CallbackContext context);

		void OnFire(InputAction.CallbackContext context);

		void OnInteract(InputAction.CallbackContext context);
	}

	private readonly InputActionMap m_Companion;

	private List<ICompanionActions> m_CompanionActionsCallbackInterfaces = new List<ICompanionActions>();

	private readonly InputAction m_Companion_Move;

	private readonly InputAction m_Companion_Jump;

	private readonly InputAction m_Companion_Sprint;

	private readonly InputAction m_Companion_Meow;

	private readonly InputAction m_Companion_Fire;

	private readonly InputAction m_Companion_Interact;

	private int m_KeyboardMouseSchemeIndex = -1;

	private int m_GamepadSchemeIndex = -1;

	public InputActionAsset asset { get; }

	public InputBinding? bindingMask
	{
		get
		{
			return asset.bindingMask;
		}
		set
		{
			asset.bindingMask = value;
		}
	}

	public ReadOnlyArray<InputDevice>? devices
	{
		get
		{
			return asset.devices;
		}
		set
		{
			asset.devices = value;
		}
	}

	public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

	public IEnumerable<InputBinding> bindings => asset.bindings;

	public CompanionActions Companion => new CompanionActions(this);

	public InputControlScheme KeyboardMouseScheme
	{
		get
		{
			if (m_KeyboardMouseSchemeIndex == -1)
			{
				m_KeyboardMouseSchemeIndex = asset.FindControlSchemeIndex("Keyboard&Mouse");
			}
			return asset.controlSchemes[m_KeyboardMouseSchemeIndex];
		}
	}

	public InputControlScheme GamepadScheme
	{
		get
		{
			if (m_GamepadSchemeIndex == -1)
			{
				m_GamepadSchemeIndex = asset.FindControlSchemeIndex("Gamepad");
			}
			return asset.controlSchemes[m_GamepadSchemeIndex];
		}
	}

	public CompanionInputActions()
	{
		asset = InputActionAsset.FromJson("{\r\n    \"version\": 1,\r\n    \"name\": \"CompanionInputActions\",\r\n    \"maps\": [\r\n        {\r\n            \"name\": \"Companion\",\r\n            \"id\": \"8f3b6db1-933a-48d7-80e1-0fd6ea749256\",\r\n            \"actions\": [\r\n                {\r\n                    \"name\": \"Move\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"991ae602-79b1-49cc-8b8d-16d1cbb58a29\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"Jump\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"036b5670-a3fd-44db-9602-f6789571d4b9\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Sprint\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"ce616db4-14b5-49d7-bd4f-846cfb180e3b\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"Meow\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"faccb922-67ff-4679-9ec1-01c517d03b84\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Fire\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"d754af6c-cad0-4a4d-bc9f-08fb0b39d005\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Interact\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"e46d528e-b30b-4735-a22c-c3577490f360\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                }\r\n            ],\r\n            \"bindings\": [\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"ea48dacf-6cb6-4686-ad1a-c9cbeada7c9a\",\r\n                    \"path\": \"<Gamepad>/leftStick\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"StickDeadzone\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"WASD\",\r\n                    \"id\": \"5d43b4e0-218b-44dc-b3cb-2dd9e63dd7dc\",\r\n                    \"path\": \"Dpad\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"0a52543c-1a8e-438b-9201-5cc3d04778f9\",\r\n                    \"path\": \"<Keyboard>/w\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"244295eb-8363-4ba4-854d-301e4556ead7\",\r\n                    \"path\": \"<Keyboard>/upArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"e800ec6c-736f-4e4e-9010-7e2c3554b51c\",\r\n                    \"path\": \"<Keyboard>/s\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"1fd77ae5-b1c2-4d73-bae8-c45ca3677373\",\r\n                    \"path\": \"<Keyboard>/downArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"b0210420-0150-4a14-9599-9d4ac1fcd7a7\",\r\n                    \"path\": \"<Keyboard>/a\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"5cbffc1b-ec10-4309-88ca-ab62c339a549\",\r\n                    \"path\": \"<Keyboard>/leftArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"ca0c2df4-7593-4e18-94fd-5beadb44ba34\",\r\n                    \"path\": \"<Keyboard>/d\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"bd94cd34-a491-4a14-a7de-8a8c98c4dc69\",\r\n                    \"path\": \"<Keyboard>/rightArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"d96e63a0-c7d5-40b7-89ca-720c7bfaa67a\",\r\n                    \"path\": \"<Keyboard>/space\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Jump\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"eea7a120-d92f-41c7-bd10-8d89612fd053\",\r\n                    \"path\": \"<Gamepad>/{PrimaryAction}\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"Jump\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"30426564-a8df-4c9a-8c9f-f589dbf7d066\",\r\n                    \"path\": \"<Keyboard>/leftShift\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Sprint\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"85d76c4b-562b-4d6d-95e7-e47595b3582a\",\r\n                    \"path\": \"<Gamepad>/rightShoulder\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"Sprint\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"830b0905-1239-4b3e-9b19-0875f816b2cd\",\r\n                    \"path\": \"<Keyboard>/r\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Meow\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"4e2c8760-c4e4-4b7b-9cba-254ce0f72e9f\",\r\n                    \"path\": \"<Gamepad>/buttonNorth\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"Meow\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"da226f86-ae75-4202-8122-fe11bf16aa62\",\r\n                    \"path\": \"<Gamepad>/rightTrigger\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"Fire\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"152e37c4-4a21-46bb-a8c6-80ec83d40040\",\r\n                    \"path\": \"<Mouse>/leftButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Fire\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"353f0d52-a31e-43b3-b0e4-73a2813238ff\",\r\n                    \"path\": \"<Gamepad>/buttonWest\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Gamepad\",\r\n                    \"action\": \"Interact\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"4712919d-a117-4548-841e-3acf9f3e53f7\",\r\n                    \"path\": \"<Keyboard>/e\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Interact\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                }\r\n            ]\r\n        }\r\n    ],\r\n    \"controlSchemes\": [\r\n        {\r\n            \"name\": \"Keyboard&Mouse\",\r\n            \"bindingGroup\": \"Keyboard&Mouse\",\r\n            \"devices\": [\r\n                {\r\n                    \"devicePath\": \"<Keyboard>\",\r\n                    \"isOptional\": false,\r\n                    \"isOR\": false\r\n                },\r\n                {\r\n                    \"devicePath\": \"<Mouse>\",\r\n                    \"isOptional\": false,\r\n                    \"isOR\": false\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"Gamepad\",\r\n            \"bindingGroup\": \"Gamepad\",\r\n            \"devices\": [\r\n                {\r\n                    \"devicePath\": \"<Gamepad>\",\r\n                    \"isOptional\": false,\r\n                    \"isOR\": false\r\n                }\r\n            ]\r\n        }\r\n    ]\r\n}");
		m_Companion = asset.FindActionMap("Companion", throwIfNotFound: true);
		m_Companion_Move = m_Companion.FindAction("Move", throwIfNotFound: true);
		m_Companion_Jump = m_Companion.FindAction("Jump", throwIfNotFound: true);
		m_Companion_Sprint = m_Companion.FindAction("Sprint", throwIfNotFound: true);
		m_Companion_Meow = m_Companion.FindAction("Meow", throwIfNotFound: true);
		m_Companion_Fire = m_Companion.FindAction("Fire", throwIfNotFound: true);
		m_Companion_Interact = m_Companion.FindAction("Interact", throwIfNotFound: true);
	}

	~CompanionInputActions()
	{
	}

	public void Dispose()
	{
		UnityEngine.Object.Destroy(asset);
	}

	public bool Contains(InputAction action)
	{
		return asset.Contains(action);
	}

	public IEnumerator<InputAction> GetEnumerator()
	{
		return asset.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Enable()
	{
		asset.Enable();
	}

	public void Disable()
	{
		asset.Disable();
	}

	public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
	{
		return asset.FindAction(actionNameOrId, throwIfNotFound);
	}

	public int FindBinding(InputBinding bindingMask, out InputAction action)
	{
		return asset.FindBinding(bindingMask, out action);
	}
}
