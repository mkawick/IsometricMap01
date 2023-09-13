using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;

/*public class DrawingNodeTest : MonoBehaviour
{
    Camera camera;
    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}*/

[ExecuteAlways]
public class DrawingNodeTest : ImmediateModeShapeDrawer 
{

	public override void DrawShapes(Camera cam)
	{

		using (Draw.Command(cam))
		{

			// set up static parameters. these are used for all following Draw.Line calls
			Draw.LineGeometry = LineGeometry.Volumetric3D;
			Draw.ThicknessSpace = ThicknessSpace.Pixels;
			Draw.Thickness = 4; // 4px wide

			// set static parameter to draw in the local space of this object
			Draw.Matrix = transform.localToWorldMatrix;

			// draw lines
			/*	Draw.Line(Vector3.zero, Vector3.right, Color.red);
				Draw.Line(Vector3.zero, Vector3.up, Color.green);
				Draw.Line(Vector3.zero, Vector3.forward, Color.blue);*/
			var rect = new Rect(Vector3.zero, new Vector3(1, 1, 1));
			Draw.Rectangle(rect, Color.blue);
		}

	}

	void OnDrawGizmos()
	{
		// set up all static parameters. these are used for all following Draw.Line calls
		Draw.LineGeometry = LineGeometry.Volumetric3D;
		Draw.ThicknessSpace = ThicknessSpace.Pixels;
		Draw.Thickness = 4; // 4px wide

		// draw lines
		/*	Draw.Line(Vector3.zero, Vector3.right, Color.red);
			Draw.Line(Vector3.zero, Vector3.up, Color.green);
			Draw.Line(Vector3.zero, Vector3.forward, Color.blue);*/
		var rect = new Rect(Vector3.zero, new Vector3(1, 0, 1));
		Draw.Rectangle(rect, Color.blue);
	}
}