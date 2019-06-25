// 3drendercpp.cpp : This file contains the 'main' function. Program execution begins and ends there.
//
//#include "olcConsoleGameEngine.h"

#include <fstream>
#include <strstream>
#include <algorithm>

using namespace std;

#define OLC_PGE_APPLICATION
#include "olcPixelGameEngine.h"


struct vec3d {
	float x = 0;
	float y = 0;
	float z = 0;
	float w = 1;
};

struct triangle {
	vec3d p[3];
	
	olc::Pixel col;
};



struct mesh {
	vector<triangle> tris;

	bool LoadFromObjectFile(string fileName) {
		ifstream f(fileName);
		if (!f.is_open()) {
			return false;
		}
		vector<vec3d> verts;
		while (!f.eof()) {
			char line[128];
			f.getline(line, 128);

			strstream s;
			s << line;
			char junk;

			if (line[0] == 'v') { // vertex
				vec3d v;
				s >> junk >> v.x >> v.y >> v.z;
				verts.push_back(v);

			}
			else if (line[0] == 'f') {
				int f[3];
				s >> junk >> f[0] >> f[1] >> f[2];
				vec3d a = verts[f[0] - 1];
				vec3d b = verts[f[1] - 1];
				vec3d c = verts[f[2] - 1];
				triangle tri = { a,b,c };
				tris.push_back(tri);
			}
		}
		return true;
	}
};

struct mat4x4 {
	float m[4][4] = { 0 };
};



class olcEngine3D : public olc::PixelGameEngine {
public:
	float const PI = 3.14159265f;
	float const HALF_PI = PI / 2.0f;
	olcEngine3D() {
		sAppName = "3D Demo";

	}

	//static vec3d GetNormalised(vec3d A) {
	//	float nMag = sqrt(A.x * A.x + A.y * A.y + A.z * A.z);
	//	vec3d n;
	//	n.x = A.x / nMag;
	//	n.y = A.y / nMag;
	//	n.z = A.z / nMag;

	//	return n;
	//}

	//static float Dot(vec3d A, vec3d B) {
	//	return A.x * B.x + A.y * B.y + A.z * B.z;
	//}

private:
	mesh meshCube;
	mat4x4 matProj;
	vec3d vCamera;
	vec3d vLookDir;

	float fTheta;
	float fYaw;

	vec3d Matrix_MultiplyVector(mat4x4 &m, vec3d &i) {
		vec3d v;
		v.x = i.x * m.m[0][0] + i.y * m.m[1][0] + i.z * m.m[2][0] + i.w * m.m[3][0];
		v.y = i.x * m.m[0][1] + i.y * m.m[1][1] + i.z * m.m[2][1] + i.w * m.m[3][1];
		v.z = i.x * m.m[0][2] + i.y * m.m[1][2] + i.z * m.m[2][2] + i.w * m.m[3][2];
		v.w = i.x * m.m[0][3] + i.y * m.m[1][3] + i.z * m.m[2][3] + i.w * m.m[3][3];

		return v;
	}

	mat4x4 Matrix_MakeIdentity() {
		mat4x4 matrix;

		matrix.m[0][0] = 1.0f;
		matrix.m[1][1] = 1.0f;
		matrix.m[2][2] = 1.0f;
		matrix.m[3][3] = 1.0f;

		return matrix;
	}

	mat4x4 Matrix_MakeRotationX(float fAngleRad)
	{
		mat4x4 matrix;
		matrix.m[0][0] = 1.0f;
		matrix.m[1][1] = cosf(fAngleRad);
		matrix.m[1][2] = sinf(fAngleRad);
		matrix.m[2][1] = -sinf(fAngleRad);
		matrix.m[2][2] = cosf(fAngleRad);
		matrix.m[3][3] = 1.0f;
		return matrix;
	}

	mat4x4 Matrix_MakeRotationY(float fAngleRad)
	{
		mat4x4 matrix;
		matrix.m[0][0] = cosf(fAngleRad);
		matrix.m[0][2] = sinf(fAngleRad);
		matrix.m[2][0] = -sinf(fAngleRad);
		matrix.m[1][1] = 1.0f;
		matrix.m[2][2] = cosf(fAngleRad);
		matrix.m[3][3] = 1.0f;
		return matrix;
	}

	mat4x4 Matrix_MakeRotationZ(float fAngleRad)
	{
		mat4x4 matrix;
		matrix.m[0][0] = cosf(fAngleRad);
		matrix.m[0][1] = sinf(fAngleRad);
		matrix.m[1][0] = -sinf(fAngleRad);
		matrix.m[1][1] = cosf(fAngleRad);
		matrix.m[2][2] = 1.0f;
		matrix.m[3][3] = 1.0f;
		return matrix;
	}

	mat4x4 Matrix_MakeTranslation(float x, float y, float z)
	{
		mat4x4 matrix;
		matrix.m[0][0] = 1.0f;
		matrix.m[1][1] = 1.0f;
		matrix.m[2][2] = 1.0f;
		matrix.m[3][3] = 1.0f;
		matrix.m[3][0] = x;
		matrix.m[3][1] = y;
		matrix.m[3][2] = z;
		return matrix;
	}

	mat4x4 Matrix_MakeProjection(float fFovDegrees, float fAspectRatio, float fNear, float fFar)
	{
		float fFovRad = 1.0f / tanf(fFovDegrees * 0.5f / 180.0f * 3.14159f);
		mat4x4 matrix;
		matrix.m[0][0] = fAspectRatio * fFovRad;
		matrix.m[1][1] = fFovRad;
		matrix.m[2][2] = fFar / (fFar - fNear);
		matrix.m[3][2] = (-fFar * fNear) / (fFar - fNear);
		matrix.m[2][3] = 1.0f;
		matrix.m[3][3] = 0.0f;
		return matrix;
	}

	mat4x4 Matrix_MultiplyMatrix(mat4x4 &m1, mat4x4 &m2) {
		mat4x4 matrix;
		for (int c = 0; c < 4; c += 1) {
			for (int r = 0; r < 4; r += 1) {
				matrix.m[r][c] = m1.m[r][0] * m2.m[0][c] + m1.m[r][1] * m2.m[1][c] + m1.m[r][2] * m2.m[2][c] + m1.m[r][3] * m2.m[3][c];
			}
		}

		return matrix;
	}

	vec3d Vector_Add(vec3d &v1, vec3d &v2) {
		return { v1.x + v2.x, v1.y + v2.y, v1.z + v2.z };
	}

	vec3d Vector_Sub(vec3d &v1, vec3d &v2) {
		return { v1.x - v2.x, v1.y - v2.y, v1.z - v2.z };
	}

	vec3d Vector_Mult(vec3d &v1, float k) {
		return { v1.x * k, v1.y * k, v1.z * k };
	}

	vec3d Vector_Div(vec3d &v1, float k) {
		return { v1.x / k, v1.y / k, v1.z / k };
	}

	float Vector_Dot(vec3d &v1, vec3d &v2) {
		return v1.x*v2.x + v1.y*v2.y + v1.z * v2.z;
	}

	float Vector_Mag(vec3d &v) {
		return sqrtf(Vector_Dot(v, v));
	}

	vec3d Vector_Normalise(vec3d &v) {
		float mag = Vector_Mag(v);
		return { v.x / mag, v.y / mag, v.z / mag };
	}

	vec3d Vector_Cross(vec3d &v1, vec3d &v2) {
		vec3d v;
		v.x = v1.y * v2.z - v1.z * v2.y;
		v.y = v1.z * v2.x - v1.x * v2.z;
		v.z = v1.x * v2.y - v1.y * v2.x;
		return v;
	}

	float Lerp(float firstFloat, float secondFloat, float by)
	{
		return firstFloat * (1 - by) + secondFloat * by;
	}

	mat4x4 Matrix_PointAt(vec3d &pos, vec3d &target, vec3d &up) {
		vec3d newForward = Vector_Sub(target, pos);

		newForward = Vector_Normalise(newForward);
		vec3d a = Vector_Mult(newForward, Vector_Dot(up, newForward));
		vec3d newUp = Vector_Sub(up, a);
		newUp = Vector_Normalise(newUp);

		vec3d newRight = Vector_Cross(newUp, newForward);

		// Construct Dimensioning and Translation Matrix	
		mat4x4 matrix;
		matrix.m[0][0] = newRight.x;	matrix.m[0][1] = newRight.y;	matrix.m[0][2] = newRight.z;	matrix.m[0][3] = 0.0f;
		matrix.m[1][0] = newUp.x;		matrix.m[1][1] = newUp.y;		matrix.m[1][2] = newUp.z;		matrix.m[1][3] = 0.0f;
		matrix.m[2][0] = newForward.x;	matrix.m[2][1] = newForward.y;	matrix.m[2][2] = newForward.z;	matrix.m[2][3] = 0.0f;
		matrix.m[3][0] = pos.x;			matrix.m[3][1] = pos.y;			matrix.m[3][2] = pos.z;			matrix.m[3][3] = 1.0f;
		return matrix;
	}

	mat4x4 Matrix_QuickInverse(mat4x4 &m) // Only for Rotation/Translation Matrices
	{
		mat4x4 matrix;
		matrix.m[0][0] = m.m[0][0]; matrix.m[0][1] = m.m[1][0]; matrix.m[0][2] = m.m[2][0]; matrix.m[0][3] = 0.0f;
		matrix.m[1][0] = m.m[0][1]; matrix.m[1][1] = m.m[1][1]; matrix.m[1][2] = m.m[2][1]; matrix.m[1][3] = 0.0f;
		matrix.m[2][0] = m.m[0][2]; matrix.m[2][1] = m.m[1][2]; matrix.m[2][2] = m.m[2][2]; matrix.m[2][3] = 0.0f;
		matrix.m[3][0] = -(m.m[3][0] * matrix.m[0][0] + m.m[3][1] * matrix.m[1][0] + m.m[3][2] * matrix.m[2][0]);
		matrix.m[3][1] = -(m.m[3][0] * matrix.m[0][1] + m.m[3][1] * matrix.m[1][1] + m.m[3][2] * matrix.m[2][1]);
		matrix.m[3][2] = -(m.m[3][0] * matrix.m[0][2] + m.m[3][1] * matrix.m[1][2] + m.m[3][2] * matrix.m[2][2]);
		matrix.m[3][3] = 1.0f;
		return matrix;
	}



	//CHAR_INFO GetColour(float lum)
	//{
	//	short bg_col, fg_col;
	//	wchar_t sym;
	//	int pixel_bw = (int)(13.0f*lum);
	//	switch (pixel_bw)
	//	{
	//	case 0: bg_col = BG_BLACK; fg_col = FG_BLACK; sym = PIXEL_SOLID; break;

	//	case 1: bg_col = BG_BLACK; fg_col = FG_DARK_GREY; sym = PIXEL_QUARTER; break;
	//	case 2: bg_col = BG_BLACK; fg_col = FG_DARK_GREY; sym = PIXEL_HALF; break;
	//	case 3: bg_col = BG_BLACK; fg_col = FG_DARK_GREY; sym = PIXEL_THREEQUARTERS; break;
	//	case 4: bg_col = BG_BLACK; fg_col = FG_DARK_GREY; sym = PIXEL_SOLID; break;

	//	case 5: bg_col = BG_DARK_GREY; fg_col = FG_GREY; sym = PIXEL_QUARTER; break;
	//	case 6: bg_col = BG_DARK_GREY; fg_col = FG_GREY; sym = PIXEL_HALF; break;
	//	case 7: bg_col = BG_DARK_GREY; fg_col = FG_GREY; sym = PIXEL_THREEQUARTERS; break;
	//	case 8: bg_col = BG_DARK_GREY; fg_col = FG_GREY; sym = PIXEL_SOLID; break;

	//	case 9:  bg_col = BG_GREY; fg_col = FG_WHITE; sym = PIXEL_QUARTER; break;
	//	case 10: bg_col = BG_GREY; fg_col = FG_WHITE; sym = PIXEL_HALF; break;
	//	case 11: bg_col = BG_GREY; fg_col = FG_WHITE; sym = PIXEL_THREEQUARTERS; break;
	//	case 12: bg_col = BG_GREY; fg_col = FG_WHITE; sym = PIXEL_SOLID; break;
	//	default:
	//		bg_col = BG_BLACK; fg_col = FG_BLACK; sym = PIXEL_SOLID;
	//	}

	//	CHAR_INFO c;
	//	c.Attributes = bg_col | fg_col;
	//	c.Char.UnicodeChar = sym;
	//	return c;
	//}

public:

	bool OnUserCreate() override {

		//meshCube.tris = {

		//	// SOUTH
		//	{ 0.0f, 0.0f, 0.0f,    0.0f, 1.0f, 0.0f,    1.0f, 1.0f, 0.0f },
		//	{ 0.0f, 0.0f, 0.0f,    1.0f, 1.0f, 0.0f,    1.0f, 0.0f, 0.0f },

		//	// EAST                                                      
		//	{ 1.0f, 0.0f, 0.0f,    1.0f, 1.0f, 0.0f,    1.0f, 1.0f, 1.0f },
		//	{ 1.0f, 0.0f, 0.0f,    1.0f, 1.0f, 1.0f,    1.0f, 0.0f, 1.0f },

		//	// NORTH                                                     
		//	{ 1.0f, 0.0f, 1.0f,    1.0f, 1.0f, 1.0f,    0.0f, 1.0f, 1.0f },
		//	{ 1.0f, 0.0f, 1.0f,    0.0f, 1.0f, 1.0f,    0.0f, 0.0f, 1.0f },

		//	// WEST                                                      
		//	{ 0.0f, 0.0f, 1.0f,    0.0f, 1.0f, 1.0f,    0.0f, 1.0f, 0.0f },
		//	{ 0.0f, 0.0f, 1.0f,    0.0f, 1.0f, 0.0f,    0.0f, 0.0f, 0.0f },

		//	// TOP                                                       
		//	{ 0.0f, 1.0f, 0.0f,    0.0f, 1.0f, 1.0f,    1.0f, 1.0f, 1.0f },
		//	{ 0.0f, 1.0f, 0.0f,    1.0f, 1.0f, 1.0f,    1.0f, 1.0f, 0.0f },

		//	// BOTTOM                                                    
		//	{ 1.0f, 0.0f, 1.0f,    0.0f, 0.0f, 1.0f,    0.0f, 0.0f, 0.0f },
		//	{ 1.0f, 0.0f, 1.0f,    0.0f, 0.0f, 0.0f,    1.0f, 0.0f, 0.0f },

		//};

		meshCube.LoadFromObjectFile("axis.obj");

		//projection matrix
		float fNear = 0.1f;
		float fFar = 1000;
		float fFov = 90;

		float fAspectRatio = (float)ScreenHeight() / (float)ScreenWidth();


		matProj = Matrix_MakeProjection(fFov, fAspectRatio, fNear, fFar);
		/*matProj.m[0][0] = fAspectRatio * fFovRad;
		matProj.m[1][1] = fFovRad;
		matProj.m[2][2] = fFar / (fFar - fNear);
		matProj.m[3][2] = (-fFar * fNear) / (fFar - fNear);
		matProj.m[2][3] = 1.0f;
		matProj.m[3][3] = 0.0f;*/




		return true;
	}

	bool OnUserUpdate(float fElapsedTime) override {

		if (GetKey(olc::Key::UP).bHeld) {
			vCamera.y += 8.0f * fElapsedTime;
		}

		if (GetKey(olc::Key::DOWN).bHeld) {
			vCamera.y -= 8.0f * fElapsedTime;
		}

		if (GetKey(olc::Key::LEFT).bHeld) {
			vCamera.x -= 8.0f * fElapsedTime;
		}

		if (GetKey(olc::Key::RIGHT).bHeld) {
			vCamera.x += 8.0f * fElapsedTime;
		}

		vec3d vForward = Vector_Mult(vLookDir, 8.0f * fElapsedTime);

		if (GetKey(olc::Key::A).bHeld) {
			fYaw -= 2.0f * fElapsedTime;
		}

		if (GetKey(olc::Key::F).bHeld) {
			fYaw += 2.0f * fElapsedTime;
		}
		
		



		float sw = (float)ScreenWidth();
		float sh = (float)ScreenHeight();
		FillRect(0, 0, ScreenWidth(), ScreenHeight(), olc::BLACK);

		mat4x4 matRotZ, matRotX;
		//fTheta += fElapsedTime;

		matRotZ = Matrix_MakeRotationZ(fTheta * 0.5f);
		matRotX = Matrix_MakeRotationX(fTheta);

		mat4x4 matTrans;
		matTrans = Matrix_MakeTranslation(0, 0, 10.0f);

		mat4x4 matWorld;
		matWorld = Matrix_MakeIdentity();
		matWorld = Matrix_MultiplyMatrix(matWorld, matRotZ);
		matWorld = Matrix_MultiplyMatrix(matWorld, matRotX);
		matWorld = Matrix_MultiplyMatrix(matWorld, matTrans);

		vLookDir = { 0,0,1 };
		vec3d vUp = { 0,1,0 };
		vec3d vTarget = Vector_Add(vCamera, vLookDir);
		mat4x4 matCamera = Matrix_PointAt(vCamera, vTarget, vUp);

		mat4x4 matView = Matrix_QuickInverse(matCamera);
		//matRotZ.m[0][0] = cosf(fTheta);
		//matRotZ.m[0][1] = sinf(fTheta);
		//matRotZ.m[1][0] = -sinf(fTheta);
		//matRotZ.m[1][1] = cosf(fTheta);
		//matRotZ.m[2][2] = 1;
		//matRotZ.m[3][3] = 1;

		//matRotX.m[0][0] = 1;
		//matRotX.m[1][1] = cosf(fTheta * 0.5f);
		//matRotX.m[1][2] = sinf(fTheta* 0.5f);
		//matRotX.m[2][1] = -sinf(fTheta* 0.5f);
		//matRotX.m[2][2] = cosf(fTheta* 0.5f);
		//matRotX.m[3][3] = 1;

		vector<triangle> triangles;


		for (auto tri : meshCube.tris) {
			triangle triProjected, triTransformed, triViewed; // , triTranslated, triRotatedZ, triRotatedZX;

			triTransformed.p[0] = Matrix_MultiplyVector(matWorld, tri.p[0]);
			triTransformed.p[1] = Matrix_MultiplyVector(matWorld, tri.p[1]);
			triTransformed.p[2] = Matrix_MultiplyVector(matWorld, tri.p[2]);


			vec3d normal, line1, line2;
			line1 = Vector_Sub(triTransformed.p[1], triTransformed.p[0]);
			line2 = Vector_Sub(triTransformed.p[2], triTransformed.p[0]);

			normal = Vector_Cross(line1, line2);

			normal = Vector_Normalise(normal);
			/*float nMag = sqrt(normal.x * normal.x + normal.y * normal.y + normal.z * normal.z);
			normal.x /= nMag;
			normal.y /= nMag;
			normal.z /= nMag;*/

			vec3d vCamRay = Vector_Sub(triTransformed.p[0], vCamera);



			//if (normal.z < 0) 
			if (Vector_Dot(normal, vCamRay) < 0.0f) {


				vec3d directionalLight = { 0.0f, 1.0f, -1.0f };
				directionalLight = Vector_Normalise(directionalLight);

				float dp = Vector_Dot(normal, directionalLight);
				//CHAR_INFO c = GetColour(dp);
				float col = Lerp(0, 255, dp);
				triTransformed.col = olc::Pixel(col, col, col);
				
				//triTransformed.sym = c.Char.UnicodeChar;

				//world space to view space
				triViewed.p[0] = Matrix_MultiplyVector(matView, triTransformed.p[0]);
				triViewed.p[1] = Matrix_MultiplyVector(matView, triTransformed.p[1]);
				triViewed.p[2] = Matrix_MultiplyVector(matView, triTransformed.p[2]);

				//project
				triProjected.p[0] = Matrix_MultiplyVector(matProj, triViewed.p[0]);
				triProjected.p[1] = Matrix_MultiplyVector(matProj, triViewed.p[1]);
				triProjected.p[2] = Matrix_MultiplyVector(matProj, triViewed.p[2]);

				triProjected.col = triTransformed.col;
				//triProjected.sym = triTransformed.sym;

				triProjected.p[0] = Vector_Div(triProjected.p[0], triProjected.p[0].w);
				triProjected.p[1] = Vector_Div(triProjected.p[1], triProjected.p[1].w);
				triProjected.p[2] = Vector_Div(triProjected.p[2], triProjected.p[2].w);

				// Scale into view
				vec3d vOffsetView = { 1,1,0 };

				triProjected.p[0] = Vector_Add(triProjected.p[0], vOffsetView);
				triProjected.p[1] = Vector_Add(triProjected.p[1], vOffsetView);
				triProjected.p[2] = Vector_Add(triProjected.p[2], vOffsetView);

				triProjected.p[0].x *= 0.5f * (float)ScreenWidth();
				triProjected.p[0].y *= 0.5f * (float)ScreenHeight();
				triProjected.p[1].x *= 0.5f * (float)ScreenWidth();
				triProjected.p[1].y *= 0.5f * (float)ScreenHeight();
				triProjected.p[2].x *= 0.5f * (float)ScreenWidth();
				triProjected.p[2].y *= 0.5f * (float)ScreenHeight();
				triangles.push_back(triProjected);
				//FillTriangle(triProjected.p[0].x, triProjected.p[0].y,
				//	triProjected.p[1].x, triProjected.p[1].y,
				//	triProjected.p[2].x, triProjected.p[2].y,
				//	triProjected.sym, triProjected.col);

				//DrawTriangle(triProjected.p[0].x, triProjected.p[0].y,
				//	triProjected.p[1].x, triProjected.p[1].y,
				//	triProjected.p[2].x, triProjected.p[2].y,
				//	PIXEL_SOLID, FG_BLACK);
			}
		}

		//sort triangles
		sort(triangles.begin(), triangles.end(), [](triangle &t1, triangle &t2) {
			float z1 = (t1.p[0].z + t1.p[1].z + t1.p[2].z) / 3.0f;
			float z2 = (t2.p[0].z + t2.p[1].z + t2.p[2].z) / 3.0f;

			return z1 > z2;

		});



		for (auto &triProjected : triangles) {
			FillTriangle(triProjected.p[0].x, triProjected.p[0].y,
				triProjected.p[1].x, triProjected.p[1].y,
				triProjected.p[2].x, triProjected.p[2].y,
				triProjected.col);

			//DrawTriangle(triProjected.p[0].x, triProjected.p[0].y,
			//	triProjected.p[1].x, triProjected.p[1].y,
			//	triProjected.p[2].x, triProjected.p[2].y,
			//	PIXEL_SOLID, FG_BLACK);
		}

		return true;
	}
};


int main() {
	olcEngine3D demo;

	if (demo.Construct(256, 240, 2, 2)) {
		demo.Start();
	}
	else {

	}

	return 0;
}

