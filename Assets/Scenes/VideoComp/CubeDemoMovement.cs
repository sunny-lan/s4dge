using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using v2;

public class CubeDemoMovement : MonoBehaviour
{
	private Transform4D t4d;

	// Start is called before the first frame update
	void Start()
	{
		t4d = GetComponent<Transform4D>();
		StartCoroutine(run());
	}

	IEnumerator run()
	{
		var rate = 1.2f;
		var waitBetween = 1f;
		if (true)
		{
			yield return new WaitForSeconds(waitBetween);

			for (float prog = 0; prog <= 1; prog += 3 / 360f)
			{
				float rad = Mathf.Lerp(0, 2 * Mathf.PI, prog);
				//xy->xy
				//xz->xw
				t4d.localRotation[(int)Rot4D.xy] = t4d.localRotation[(int)Rot4D.xw] = rad;
				yield return new WaitForFixedUpdate();
			}

			yield return new WaitForSeconds(waitBetween);

			for (float prog = 0; prog <= 1; prog += 3 / 360f)
			{
				float rad = Mathf.Lerp(0, 2 * Mathf.PI, prog);
				//xz->xw
				//yz->yw
				t4d.localRotation[(int)Rot4D.xw] = t4d.localRotation[(int)Rot4D.yw] = rad;
				yield return new WaitForFixedUpdate();
			}

			yield return new WaitForSeconds(waitBetween);

			for (float prog = 0; prog <= 1; prog += 3 / 360f)
			{
				float rad = Mathf.Lerp(0, 2 * Mathf.PI, prog);
				//xy->xy
				//yz->yw
				t4d.localRotation[(int)Rot4D.xy] = t4d.localRotation[(int)Rot4D.yw] = rad;
				yield return new WaitForFixedUpdate();
			}

			yield return new WaitForSeconds(waitBetween);

			for (float prog = 0; prog <= 1; prog += 3 / 360f)
			{
				float rad = Mathf.Lerp(0, 2 * Mathf.PI, prog);
				t4d.localRotation[(int)Rot4D.xy] = t4d.localRotation[(int)Rot4D.xw] = t4d.localRotation[(int)Rot4D.yw] = rad;
				yield return new WaitForFixedUpdate();
			}
		}
		if (true)
		{
			yield return new WaitForSeconds(waitBetween);

			for (float prog = 0; prog <= 1; prog += rate / 360f)
			{
				float rad = Mathf.Lerp(0, 2 * Mathf.PI, prog);
				//xw
				t4d.localRotation[(int)Rot4D.xz] = rad;
				yield return new WaitForFixedUpdate();
			}

			yield return new WaitForSeconds(waitBetween);

			for (float prog = 0; prog <= 1; prog += rate / 360f)
			{
				float rad = Mathf.Lerp(0, 2 * Mathf.PI, prog);
				//xw
				t4d.localRotation[(int)Rot4D.yz] = rad;
				yield return new WaitForFixedUpdate();
			}
			yield return new WaitForSeconds(waitBetween);

			for (float prog = 0; prog <= 1; prog += rate / 360f)
			{
				float rad = Mathf.Lerp(0, 2 * Mathf.PI, prog);
				//xw
				t4d.localRotation[(int)Rot4D.zw] = rad;
				yield return new WaitForFixedUpdate();
			}
		}
		if (true)
		{

			yield return new WaitForSeconds(waitBetween);

			for (float prog = 0; prog <= 1; prog += rate / 360f)
			{
				float rad = Mathf.Lerp(0, 2 * Mathf.PI, prog);
				//xy, xw
				t4d.localRotation[(int)Rot4D.xy] = t4d.localRotation[(int)Rot4D.xz] = rad;
				yield return new WaitForFixedUpdate();
			}

			yield return new WaitForSeconds(waitBetween);

			for (float prog = 0; prog <= 1; prog += rate / 360f)
			{
				float rad = Mathf.Lerp(0, 2 * Mathf.PI, prog);
				//xy, xw
				t4d.localRotation[(int)Rot4D.xy] = t4d.localRotation[(int)Rot4D.yz] = rad;
				yield return new WaitForFixedUpdate();
			}


			yield return new WaitForSeconds(waitBetween);

			for (float prog = 0; prog <= 1; prog += rate / 360f)
			{
				float rad = Mathf.Lerp(0, 2 * Mathf.PI, prog);
				//xy, xw
				t4d.localRotation[(int)Rot4D.xw] = t4d.localRotation[(int)Rot4D.xz] = rad;
				yield return new WaitForFixedUpdate();
			}


			yield return new WaitForSeconds(waitBetween);

			for (float prog = 0; prog <= 1; prog += rate / 360f)
			{
				float rad = Mathf.Lerp(0, 2 * Mathf.PI, prog);
				//xy, xw
				t4d.localRotation[(int)Rot4D.xw] = t4d.localRotation[(int)Rot4D.zw] = rad;
				yield return new WaitForFixedUpdate();
			}

			yield return new WaitForSeconds(waitBetween);

			for (float prog = 0; prog <= 1; prog += rate / 360f)
			{
				float rad = Mathf.Lerp(0, 2 * Mathf.PI, prog);
				//xy, xw
				t4d.localRotation[(int)Rot4D.yw] = t4d.localRotation[(int)Rot4D.yz] = rad;
				yield return new WaitForFixedUpdate();
			}

			yield return new WaitForSeconds(waitBetween);

			for (float prog = 0; prog <= 1; prog += rate / 360f)
			{
				float rad = Mathf.Lerp(0, 2 * Mathf.PI, prog);
				//xy, xw
				t4d.localRotation[(int)Rot4D.yw] = t4d.localRotation[(int)Rot4D.zw] = rad;
				yield return new WaitForFixedUpdate();
			}
		}
		if (true)
		{

			yield return new WaitForSeconds(waitBetween);

			for (float prog = 0; prog <= 1; prog += rate / 360f)
			{
				float rad = Mathf.Lerp(0, 2 * Mathf.PI, prog);
				//xw, yw
				t4d.localRotation[(int)Rot4D.xz] = t4d.localRotation[(int)Rot4D.yz] = rad;
				yield return new WaitForFixedUpdate();
			}

			yield return new WaitForSeconds(waitBetween);

			for (float prog = 0; prog <= 1; prog += rate / 360f)
			{
				float rad = Mathf.Lerp(0, 2 * Mathf.PI, prog);
				//xw, zw
				t4d.localRotation[(int)Rot4D.xz] = t4d.localRotation[(int)Rot4D.zw] = rad;
				yield return new WaitForFixedUpdate();
			}

			yield return new WaitForSeconds(waitBetween);

			for (float prog = 0; prog <= 1; prog += rate / 360f)
			{
				float rad = Mathf.Lerp(0, 2 * Mathf.PI, prog);
				//yw, zw
				t4d.localRotation[(int)Rot4D.yz] = t4d.localRotation[(int)Rot4D.zw] = rad;
				yield return new WaitForFixedUpdate();
			}
		}
		
		//double rotations
		if (true)
		{

			yield return new WaitForSeconds(waitBetween);

			for (float prog = 0; prog <= 1; prog += rate / 360f)
			{
				float rad = Mathf.Lerp(0, 2 * Mathf.PI, prog);
				//xy, zw
				t4d.localRotation[(int)Rot4D.xy] = t4d.localRotation[(int)Rot4D.zw] = rad;
				yield return new WaitForFixedUpdate();
			}

			yield return new WaitForSeconds(waitBetween);

			for (float prog = 0; prog <= 1; prog += rate / 360f)
			{
				float rad = Mathf.Lerp(0, 2 * Mathf.PI, prog);
				//yz, xw
				t4d.localRotation[(int)Rot4D.yw] = t4d.localRotation[(int)Rot4D.xz] = rad;
				yield return new WaitForFixedUpdate();
			}

			yield return new WaitForSeconds(waitBetween);

			for (float prog = 0; prog <= 1; prog += rate / 360f)
			{
				float rad = Mathf.Lerp(0, 2 * Mathf.PI, prog);
				//xz, yw
				t4d.localRotation[(int)Rot4D.xw] = t4d.localRotation[(int)Rot4D.yz] = rad;
				yield return new WaitForFixedUpdate();
			}
		}

		// 3 plane rotations
		if (true)
		{
			//xy, xz, xw
			//xy xw yw
			//xw xz yz
			//xy yw zw

			yield return new WaitForSeconds(waitBetween);

			for (float prog = 0; prog <= 1; prog += rate / 360f)
			{
				float rad = Mathf.Lerp(0, 2 * Mathf.PI, prog);
				t4d.localRotation[(int)Rot4D.xy] = t4d.localRotation[(int)Rot4D.xw] = t4d.localRotation[(int)Rot4D.xz] = rad;
				yield return new WaitForFixedUpdate();
			}


			yield return new WaitForSeconds(waitBetween);

			for (float prog = 0; prog <= 1; prog += rate / 360f)
			{
				float rad = Mathf.Lerp(0, 2 * Mathf.PI, prog);
				t4d.localRotation[(int)Rot4D.xy] = t4d.localRotation[(int)Rot4D.xz] = t4d.localRotation[(int)Rot4D.yz] = rad;
				yield return new WaitForFixedUpdate();
			}



			yield return new WaitForSeconds(waitBetween);

			for (float prog = 0; prog <= 1; prog += rate / 360f)
			{
				float rad = Mathf.Lerp(0, 2 * Mathf.PI, prog);
				t4d.localRotation[(int)Rot4D.xz] = t4d.localRotation[(int)Rot4D.xw] = t4d.localRotation[(int)Rot4D.yw] = rad;
				yield return new WaitForFixedUpdate();
			}



			yield return new WaitForSeconds(waitBetween);

			for (float prog = 0; prog <= 1; prog += rate / 360f)
			{
				float rad = Mathf.Lerp(0, 2 * Mathf.PI, prog);
				t4d.localRotation[(int)Rot4D.xy] = t4d.localRotation[(int)Rot4D.yz] = t4d.localRotation[(int)Rot4D.zw] = rad;
				yield return new WaitForFixedUpdate();
			}
		}

		// 4 plane
		if (true)
		{

			// xy yz xz xw
			yield return new WaitForSeconds(waitBetween);

			for (float prog = 0; prog <= 1; prog += rate / 360f)
			{
				float rad = Mathf.Lerp(0, 2 * Mathf.PI, prog);
				t4d.localRotation[(int)Rot4D.xy] = t4d.localRotation[(int)Rot4D.yw] = t4d.localRotation[(int)Rot4D.xw] = t4d.localRotation[(int)Rot4D.xz] = rad;
				yield return new WaitForFixedUpdate();
			}
		}


		// 5 plane
		if (true)
		{

			// xy yz xz xw yw
			yield return new WaitForSeconds(waitBetween);

			for (float prog = 0; prog <= 1; prog += rate / 360f)
			{
				float rad = Mathf.Lerp(0, 2 * Mathf.PI, prog);
				t4d.localRotation[(int)Rot4D.xy] = t4d.localRotation[(int)Rot4D.yw] = t4d.localRotation[(int)Rot4D.xw] = t4d.localRotation[(int)Rot4D.xz] = t4d.localRotation[(int)Rot4D.yz] = rad;
				yield return new WaitForFixedUpdate();
			}
		}


		// 6 plane
		if (true)
		{

			// xy yz xz xw yw
			yield return new WaitForSeconds(waitBetween);

			for (float prog = 0; prog <= 1; prog += rate / 360f)
			{
				float rad = Mathf.Lerp(0, 2 * Mathf.PI, prog);
				t4d.localRotation[(int)Rot4D.xy] = t4d.localRotation[(int)Rot4D.yw] = t4d.localRotation[(int)Rot4D.xw] = t4d.localRotation[(int)Rot4D.xz] = t4d.localRotation[(int)Rot4D.yz] = t4d.localRotation[(int)Rot4D.zw] = rad;
				yield return new WaitForFixedUpdate();
			}
		}
	}

}
