using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class XRRigGiveMeInfo : MonoBehaviour
{
    private XRRig rig;
    public Text output;
    
    // Start is called before the first frame update
    void Start()
    {
        FindObjectOfType<XRRig>();
    }

    private FieldInfo subsystems;
    
    // Update is called once per frame
    void Update()
    {
        // reflect in
        if(subsystems == null) subsystems = typeof(XRRig).GetField("s_InputSubsystems", (BindingFlags) (-1));
        var subsystem = (List<XRInputSubsystem>) subsystems.GetValue(null);
        output.text = subsystem.Count + "\n" + string.Join("\n", subsystem.Select(x => "running: " + x.running + ", descriptor: " + x.subsystemDescriptor.id));
    }
}
