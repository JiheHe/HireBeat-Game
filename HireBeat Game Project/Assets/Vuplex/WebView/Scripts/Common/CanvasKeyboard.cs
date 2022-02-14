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
using UnityEngine;
using UnityEngine.UI;
using Vuplex.WebView.Internal;

namespace Vuplex.WebView {

    /// <summary>
    /// Like the Keyboard prefab, except optimized for use in a Canvas.
    /// You can add a CanvasKeyboard to your scene either by dragging the CanvasKeyboard.prefab file
    /// into a Canvas via the editor or by programmatically calling CanvasKeyboard.Instantiate().
    /// For an example, please see 3D WebView's CanvasWorldSpaceDemo scene.
    /// </summary>
    /// <example>
    /// <code>
    /// // Create a CanvasKeyboard.
    /// var keyboard = CanvasKeyboard.Instantiate();
    /// keyboard.transform.SetParent(canvas.transform, false);
    /// var rectTransform = keyboard.transform as RectTransform;
    /// rectTransform.anchoredPosition3D = Vector3.zero;
    /// rectTransform.offsetMin = Vector2.zero;
    /// rectTransform.offsetMax = Vector2.zero;
    /// rectTransform.sizeDelta = new Vector2(650, 162);
    /// // Hook up the keyboard so that characters are routed to a CanvasWebViewPrefab in the scene.
    /// keyboard.InputReceived += (sender, eventArgs) => {
    ///     canvasWebViewPrefab.WebView.HandleKeyboardInput(eventArgs.Value);
    /// };
    /// </code>
    /// </example>
    public class CanvasKeyboard : BaseKeyboard {

        /// <summary>
        /// Sets the keyboard's initial resolution in pixels per Unity unit.
        /// You can change the resolution to make the keyboard's content appear larger or smaller.
        /// For more information on scaling web content, see
        /// [this support article](https://support.vuplex.com/articles/how-to-scale-web-content).
        /// </summary>
        [Label("Initial Resolution (px / Unity unit)")]
        [Tooltip("You can change this to make web content appear larger or smaller.")]
        public float InitialResolution = 1;

        /// <summary>
        /// The `CanvasWebViewPrefab` instance used for the keyboard UI.
        /// </summary>
        public CanvasWebViewPrefab WebViewPrefab {
            get {
                return (CanvasWebViewPrefab)_webViewPrefab;
            }
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public static CanvasKeyboard Instantiate() {

            var prefabPrototype = (GameObject) Resources.Load("CanvasKeyboard");
            var gameObject = (GameObject) Instantiate(prefabPrototype);
            return gameObject.GetComponent<CanvasKeyboard>();
        }

        void _initCanvasKeyboard() {

            var canvasWebViewPrefab = CanvasWebViewPrefab.Instantiate(_webViewOptions);
            _webViewPrefab = canvasWebViewPrefab;
            _webViewPrefab.transform.SetParent(transform, false);
            _setLayerRecursively(_webViewPrefab.gameObject, gameObject.layer);
            var rectTransform = _webViewPrefab.transform as RectTransform;
            rectTransform.anchoredPosition3D = Vector3.zero;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            _webViewPrefab.transform.localScale = Vector3.one;
            canvasWebViewPrefab.InitialResolution = InitialResolution;
            _init();
            // Disable the image, which is just used as a placeholder in the editor.
            var image = GetComponent<Image>();
            if (image != null) {
                image.enabled = false;
            }
        }

        void Start() {

            _initCanvasKeyboard();
        }
    }
}
