using UnityEngine;
using System.Collections;

public static class CS_Global {


	public static string NAME_MESSAGEBOX = "MessageBox";
	public static string NAME_PLAYER = "Player";
	public static string NAME_LOADSTAGE = "LoadStage";

	public static string TAG_PLAYER = "Player";
	public static string TAG_FRIEND = "Friend";
	public static string TAG_WORKER = "Worker";


	public static float CAMERA_SIZE_DEFAULT = 10.0f;
	public static float CAMERA_SIZE_MAX = 60.0f;

	public static float DistanceToLine(Vector2 line_p1, Vector2 line_p2, Vector2 p3){
		//Bennett
		if (Mathf.Sqrt (
			    (line_p2.y - line_p1.y) * (line_p2.y - line_p1.y) +
			    (line_p2.x - line_p1.x) * (line_p2.x - line_p1.x)) == 0) {
			return 0;
		}
		//line_p1 and line_p2 are the line, p3 is the point
		float distance = Mathf.Abs (
			                 (line_p2.y - line_p1.y) * p3.x -
			                 (line_p2.x - line_p1.x) * p3.y +
			                 line_p2.x * line_p1.y -
			                 line_p2.y * line_p1.x
		                 ) /
		                 Mathf.Sqrt (
			                 (line_p2.y - line_p1.y) * (line_p2.y - line_p1.y) +
			                 (line_p2.x - line_p1.x) * (line_p2.x - line_p1.x)
		                 );
		return distance;
	}
}
