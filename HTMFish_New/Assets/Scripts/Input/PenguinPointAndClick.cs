// GENERATED AUTOMATICALLY FROM 'Assets/Scripts/Input/PenguinPointAndClick.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @PenguinPointAndClick : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @PenguinPointAndClick()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PenguinPointAndClick"",
    ""maps"": [
        {
            ""name"": ""MainGamePlay"",
            ""id"": ""e8220cb7-734d-4f35-8a9b-31488cbf4f88"",
            ""actions"": [
                {
                    ""name"": ""SelectGameObject"",
                    ""type"": ""Button"",
                    ""id"": ""c4e0af8b-e35d-4e71-acdc-7ea9b2fb9c9e"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""ScreenPosition"",
                    ""type"": ""Value"",
                    ""id"": ""5a5b8181-a99b-4677-aebf-acd0084024d1"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""67b6914f-7272-4b45-894f-85a5816b1b93"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SelectGameObject"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ecbd8b6b-6346-4d63-ada0-e286ab28e838"",
                    ""path"": ""<Touchscreen>/primaryTouch/tap"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SelectGameObject"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""80dbdfe9-0850-4968-9e58-6960dc3f6a5c"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ScreenPosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""8c1cf7e5-4c6c-4c08-ad1c-3d1c4e2c4130"",
                    ""path"": ""<Touchscreen>/primaryTouch/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ScreenPosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // MainGamePlay
        m_MainGamePlay = asset.FindActionMap("MainGamePlay", throwIfNotFound: true);
        m_MainGamePlay_SelectGameObject = m_MainGamePlay.FindAction("SelectGameObject", throwIfNotFound: true);
        m_MainGamePlay_ScreenPosition = m_MainGamePlay.FindAction("ScreenPosition", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

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

    // MainGamePlay
    private readonly InputActionMap m_MainGamePlay;
    private IMainGamePlayActions m_MainGamePlayActionsCallbackInterface;
    private readonly InputAction m_MainGamePlay_SelectGameObject;
    private readonly InputAction m_MainGamePlay_ScreenPosition;
    public struct MainGamePlayActions
    {
        private @PenguinPointAndClick m_Wrapper;
        public MainGamePlayActions(@PenguinPointAndClick wrapper) { m_Wrapper = wrapper; }
        public InputAction @SelectGameObject => m_Wrapper.m_MainGamePlay_SelectGameObject;
        public InputAction @ScreenPosition => m_Wrapper.m_MainGamePlay_ScreenPosition;
        public InputActionMap Get() { return m_Wrapper.m_MainGamePlay; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(MainGamePlayActions set) { return set.Get(); }
        public void SetCallbacks(IMainGamePlayActions instance)
        {
            if (m_Wrapper.m_MainGamePlayActionsCallbackInterface != null)
            {
                @SelectGameObject.started -= m_Wrapper.m_MainGamePlayActionsCallbackInterface.OnSelectGameObject;
                @SelectGameObject.performed -= m_Wrapper.m_MainGamePlayActionsCallbackInterface.OnSelectGameObject;
                @SelectGameObject.canceled -= m_Wrapper.m_MainGamePlayActionsCallbackInterface.OnSelectGameObject;
                @ScreenPosition.started -= m_Wrapper.m_MainGamePlayActionsCallbackInterface.OnScreenPosition;
                @ScreenPosition.performed -= m_Wrapper.m_MainGamePlayActionsCallbackInterface.OnScreenPosition;
                @ScreenPosition.canceled -= m_Wrapper.m_MainGamePlayActionsCallbackInterface.OnScreenPosition;
            }
            m_Wrapper.m_MainGamePlayActionsCallbackInterface = instance;
            if (instance != null)
            {
                @SelectGameObject.started += instance.OnSelectGameObject;
                @SelectGameObject.performed += instance.OnSelectGameObject;
                @SelectGameObject.canceled += instance.OnSelectGameObject;
                @ScreenPosition.started += instance.OnScreenPosition;
                @ScreenPosition.performed += instance.OnScreenPosition;
                @ScreenPosition.canceled += instance.OnScreenPosition;
            }
        }
    }
    public MainGamePlayActions @MainGamePlay => new MainGamePlayActions(this);
    public interface IMainGamePlayActions
    {
        void OnSelectGameObject(InputAction.CallbackContext context);
        void OnScreenPosition(InputAction.CallbackContext context);
    }
}
