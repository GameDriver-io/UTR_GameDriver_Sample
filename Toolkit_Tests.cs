using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
//using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using System.Threading;
using System;
using gdio.common.objects;
using gdio.unity_utr_api;

namespace Toolkit_Tests
{
    public class CoAPI_Test
    {
        // Be sure to instantiate the client outside of a test method
        CoApiClient client = null;

        [OneTimeSetUp]
        public void SetupInitialScene()
        {
            // An initial scene must be loaded manually when using PlayMode
            SceneManager.LoadScene("Splash", LoadSceneMode.Single);

            // Initialize the CoApiClient
            client = new CoApiClient();
            
            // Setup logging
            client.LoggedMessage += (s, e) =>
            {
                UnityEngine.Debug.Log(e.Message);
            };

            return;
        }
        
        // A Test behaves as an ordinary method. In this case we're connecting the the agent running in the Unity project
        [UnityTest, Order(1)]
        public IEnumerator T001_ConnectToGame()
        {
            // Define an IEnumerator
            IEnumerator<bool> result;
            
            // Perform an action that yields a result
            yield return result = client.Connect("127.0.0.1", 19734, 30);

            // Check the result is what you expect
            Assert.IsTrue(result.Current); 

        }

        [UnityTest, Order(2)]
        public IEnumerator T002_EnableHooksTest()
        {
            // Define an IEnumerator
            IEnumerator<bool> result;

            // Enable Input Hooking
            yield return result = client.EnableHooks(HookingObject.ALL);

            // Check the return value is true
            Assert.IsTrue(result.Current);
        }
        

        [UnityTest, Order(3)]
        public IEnumerator T003_WaitForobjectExistsTest()
        {
            // Define an IEnumerator
            IEnumerator<bool> result;

            // Load the Menu scene
            yield return client.LoadScene("Menu");

            // Wait for the Load_UISample object to exist in the project hierarchy
            yield return result = client.WaitForObject("//*[@name='Load_UISample']", 30);

            // Check that the result is true
            Assert.IsTrue(result.Current);
        }
        
        [UnityTest, Order(4)] //
        public IEnumerator T004_ClickObject()
        {
            // Define an IEnumerator
            IEnumerator<bool> result;

            // Load the Menu scene
            yield return client.LoadScene("Menu");

            // Wait 1 second
            yield return client.Wait(1);

            // Click the Load_UISample button with the left mouse button
            yield return result = client.ClickObject(MouseButtons.LEFT, "//*[@name='Load_UISample']", 5);

            // Example logging
            UnityEngine.Debug.Log($"ClickObject return string='{result.Current.ToString()}'");

            yield return client.Wait(1);

            // Check that the click completed successfully
            Assert.IsTrue(result.Current);
        }

        [UnityTest, Order(5)]
        public IEnumerator T005_CallMethodGenericReturnStringTest()
        {
            // Define an IEnumerator
            IEnumerator result;

            // Load The UISample Scene
            yield return client.LoadScene("UISample");

            // Call a method attached in the HipProjectManager script attached to the Canvas object, and pass in a String
            yield return result = client.CallMethod<string>("//*[@name='Canvas']/fn:component('HipProjectManager')", "LoadDetails", new string[] { "string:The Test was run on " + DateTime.Now.ToShortDateString() });

            // Log the string returned from the CallMethod command
            UnityEngine.Debug.Log($"CallMethod return string='{result.Current.ToString()}'");

            // Check that the return value from the CallMethod command returns a string
            Assert.IsTrue(result.Current.GetType().Equals(typeof(string)));
        }

        [UnityTest, Order(6)]
        public IEnumerator T006_CallMethodGenericReturnIntTest()
        {
            // Define an IEnumerator
            IEnumerator result;

            // Load The UISample scene
            yield return client.LoadScene("UISample");

            // Call a method attached in the HipProjectManager script attached to the Canvas object, and pass in 2 Int parameters
            yield return result = client.CallMethod<int>("//*[@name='Canvas']/fn:component('HipProjectManager')", "DoMath", new object[] { 1, 2 });

            // Log the Int value returned by the CallMethod command
            UnityEngine.Debug.Log($"CallMethod return int='{result.Current.ToString()}'");

            // Check the return type and value returned by the CallMethod action
            Assert.IsTrue(result.Current.GetType().Equals(typeof(int)));
            Assert.AreEqual(3, result.Current, "DoMath failed");
        }

        [UnityTest, Order(7)]
        public IEnumerator T007_ComplexObjectSerializationTest()
        {
            // Define an IEnumerator
            IEnumerator result;

            // Load the MouseMoveObject scene
            yield return client.LoadScene("MouseMoveObject");

            // Get the value of the Color component attached to the Cylinder object
            yield return result = client.GetObjectFieldValue<Color>("//*[@name='Cylinder']/fn:component('UnityEngine.Light')/@color");

            // Log the return value of the Color
            UnityEngine.Debug.Log($"GetObjectFieldValue return color={result.Current.ToString()}");

            // Check that the type of the returned value is that of a Color
            Assert.IsTrue(result.Current.GetType().Equals(typeof(Color)));
        }

        [UnityTest, Order(8)]
        public IEnumerator T008_MoveMouseToObject()
        {
            // Define an IEnumerator
            IEnumerator<bool> result;

            // Load the MouseMoveObject scene
            yield return client.LoadScene("MouseMoveObject");

            // Wait for the Cylinder object to exist in the project hierarchy
            yield return client.WaitForObject("//*[@name='Cylinder']");

            // Move the mouse to the object and wait for it to get there
            yield return result = client.MouseMoveToObject("//*[@name='Cylinder']", 30, true, true);
            UnityEngine.Debug.Log($"MouseMoveToObject return string='{result.Current.ToString()}'");

        }

        [UnityTest, Order(9)]
        public IEnumerator T009_MouseDrag()
        {
            // Define an IEnumerator
            IEnumerator result;

            // Set up the movement positions by first capturing the position of the Cylinder
            yield return result = client.GetObjectPosition("//*[@name='Cylinder']", CoordinateConversion.WorldToScreenPoint);
            UnityEngine.Debug.Log($"GetObjectPosition return string='{result.Current.ToString()}'");

            // Then create a Vector3 and set the value to the initial value of the Cylinder
            UnityEngine.Vector3 start = (UnityEngine.Vector3)result.Current;
            UnityEngine.Debug.Log($"Start set to: {start.ToString()}");

            // Then create a Vector2 and set the x,y values to +100 of that of the Cylinder position
            UnityEngine.Vector2 dest = new UnityEngine.Vector2(start.x + 100, start.y + 100);
            UnityEngine.Debug.Log($"Destination set to: {dest.ToString()}");

            // Move the object by dragging the mouse from the initial position to the destination
            yield return client.MouseDrag(MouseButtons.LEFT, dest, 100, new UnityEngine.Vector2(start.x, start.y), true);

            // Wait 1 second
            yield return client.Wait(1);

            // Check the final position and compare to the target destination, logging the values
            yield return result = client.GetObjectPosition("//*[@name='Cylinder']", CoordinateConversion.WorldToScreenPoint);
            UnityEngine.Vector3 final = (UnityEngine.Vector3)result.Current;

            UnityEngine.Debug.Log($"Mouse drag from: {start.ToString()} to: {final.ToString()}");

            Assert.AreEqual(dest.x, final.x, 2f, "Movement failed!");

        }

        [UnityTest, Order(10)]
        public IEnumerator T010_KeyboardMovement()
        {
            // Set up the test
            yield return client.LoadScene("MoveObjectScene");
            IEnumerator result;
            yield return result = client.WaitForEmptyInput();
            UnityEngine.Debug.Log($"WaitForEmptyInput return string='{result.Current.ToString()}'");

            // Get the initial position of the cube, and cast that to a new Vector3
            IEnumerator before;
            yield return before = client.GetObjectPosition("/*[@name='Cube']", CoordinateConversion.None);
            UnityEngine.Vector3 cubeStart = (UnityEngine.Vector3)before.Current;

            // Move the block down and left from origin
            yield return client.KeyPress(new UnityEngine.KeyCode[] { UnityEngine.KeyCode.DownArrow }, 100);
            yield return client.Wait(1);
            yield return client.KeyPress(new UnityEngine.KeyCode[] { UnityEngine.KeyCode.LeftArrow }, 100);
            yield return client.Wait(1);

            // Get the new position of the cube, and cast that to a new Vector3
            IEnumerator after;
            yield return after = client.GetObjectPosition("/*[@name='Cube']", CoordinateConversion.None);
            UnityEngine.Vector3 cubeEnd = (UnityEngine.Vector3)after.Current;

            // Check that the cube moved from the start
            Assert.AreNotEqual(cubeStart.x, cubeEnd.x, "Cube didn't move!");

        }

        [UnityTest, Order(98)]
        public IEnumerator T098_DisableHooksTest()
        {
            IEnumerator result = null;
            bool bDisabled = false;

            // Disable input hooking
            yield return result = client.DisableHooks(HookingObject.ALL);

            bDisabled = (bool)result.Current;

            Assert.IsTrue(bDisabled);
        }

    }
}
