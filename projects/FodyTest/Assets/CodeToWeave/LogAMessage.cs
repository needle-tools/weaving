using System;
using UnityEngine;

namespace CodeToWeave
{
    [ExecuteInEditMode]
    public class LogAMessage : MonoBehaviour
    {
        private string Message = "Original Message";

        private void OnEnable()
        {
            Debug.Log(Message);
        }

        private void Update()
        {
            Debug.Log(Message);
        }

        [ContextMenu(nameof(Log))]
        private void Log() => Debug.Log(Message);
    }
}
