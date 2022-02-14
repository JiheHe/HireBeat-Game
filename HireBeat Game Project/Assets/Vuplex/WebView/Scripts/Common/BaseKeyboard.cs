/**
* Copyright (c) 2021 Vuplex Inc. All rights reserved.
*
* Licensed under the Vuplex Commercial Software Library License, you may
* not use this file except in compliance with the License. You may obtain
* a copy of the License at
*
*     https://vuplex.com/commercial-library-license
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
using System;
using UnityEngine;

namespace Vuplex.WebView {

    public abstract class BaseKeyboard : MonoBehaviour {

        /// <summary>
        /// Indicates that the user clicked a key on the keyboard.
        /// </summary>
        public event EventHandler<EventArgs<string>> InputReceived;

        /// <summary>
        /// Indicates that the keyboard finished initializing.
        /// </summary>
        public event EventHandler Initialized;

        [SerializeField]
        [HideInInspector]
        protected BaseWebViewPrefab _webViewPrefab;

        protected static readonly WebViewOptions _webViewOptions = new WebViewOptions {
            clickWithoutStealingFocus = true,
            disableVideo = true,
            // If both Android plugins are installed, prefer the original Chromium
            // plugin for the keyboard, since the Gecko plugin doesn't support
            // transparent backgrounds.
            preferredPlugins = new WebPluginType[] { WebPluginType.Android }
        };

        protected void _init() {

            _webViewPrefab.Initialized += (sender, e) => {
                var pluginType = _webViewPrefab.WebView.PluginType;
                if (pluginType == WebPluginType.AndroidGecko) {
                    // On Android Gecko, hovering steals focus.
                    _webViewPrefab.HoveringEnabled = false;
                }
                // Scrolling and dragging can also cause the keyboard
                // to steal focus on Android Gecko, so just disable them.
                _webViewPrefab.ScrollingEnabled = false;
                _webViewPrefab.DragMode = DragMode.Disabled;
                _webViewPrefab.WebView.MessageEmitted += WebView_MessageEmitted;
                // Android Gecko and Hololens don't support transparent webviews, so set the cutout
                // rect to the entire view so that the shader makes its black background
                // pixels transparent.
                if (pluginType == WebPluginType.AndroidGecko || pluginType == WebPluginType.UniversalWindowsPlatform) {
                    _webViewPrefab.SetCutoutRect(new Rect(0, 0, 1, 1));
                }
                _webViewPrefab.WebView.LoadHtml(KeyboardUi.Html);
            };
        }

        void WebView_MessageEmitted(object sender, EventArgs<string> e) {

            var serializedMessage = e.Value;
            var messageType = JsonUtility.FromJson<BridgeMessage>(serializedMessage).type;
            switch (messageType) {
                case "keyboard.inputReceived":
                    var input = StringBridgeMessage.ParseValue(serializedMessage);
                    if (InputReceived != null) {
                        InputReceived(this, new EventArgs<string>(input));
                    }
                    break;
                case "keyboard.initialized":
                    _sendKeyboardLanguageMessage();
                    if (Initialized != null) {
                        Initialized(this, EventArgs.Empty);
                    }
                    break;
            }
        }

        string _getKeyboardLanguage() {
            switch (Application.systemLanguage) {
                case SystemLanguage.Danish:
                    return "da";
                case SystemLanguage.French:
                    return "fr";
                case SystemLanguage.German:
                    return "de";
                case SystemLanguage.Norwegian:
                    return "no";
                case SystemLanguage.Russian:
                    return "ru";
                case SystemLanguage.Spanish:
                    return "es";
                case SystemLanguage.Swedish:
                    return "sv";
                default:
                    return "en";
            }
        }

        /// <summary>
        /// Initializes the keyboard language based on the system language.
        /// </summary>
        void _sendKeyboardLanguageMessage() {

            var message = new StringBridgeMessage {
                type = "keyboard.setLanguage",
                value = _getKeyboardLanguage()
            };
            var serializedMessage = JsonUtility.ToJson(message);
            _webViewPrefab.WebView.PostMessage(serializedMessage);
        }

        protected static void _setLayerRecursively(GameObject gameObject, int layer) {

            if (gameObject == null) {
                return;
            }
            gameObject.layer = layer;
            foreach (Transform child in gameObject.transform) {
                if (child != null) {
                    _setLayerRecursively(child.gameObject, layer);
                }
            }
        }
    }
}
