using System;
using UnityEngine;

namespace CodeToWeave
{
    [ExecuteInEditMode]
    public class LogAMessage : MonoBehaviour
    {
        private string Message = "Original Message.";

        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }

        private void Update()
        {
        }

        [ContextMenu(nameof(Log))]
        private void Log() => Debug.Log(Message);
    }
}
