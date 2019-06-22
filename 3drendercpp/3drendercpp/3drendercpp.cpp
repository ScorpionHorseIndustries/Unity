// 3drendercpp.cpp : This file contains the 'main' function. Program execution begins and ends there.
//
#include "olcConsoleGameEngine.h"
#include <fstream>
#include <strstream>
#include <algorithm>
using namespace std;


struct vec3d {
	float x, y, z;



 };

struct triangle {
	vec3d p[3];
	wchar_t sym;
	short col;
	
	
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



class olcEngine3D : public olcConsoleGameEngine {
public:
	float const PI = 3.14159265f;
	float const HALF_PI = PI / 2.0f;
	olcEngine3D() {
		m_sAppName = L"3D Demo";

	}

	static vec3d GetNormalised(vec3d A) {
		float nMag = sqrt(A.x * A.x + A.y * A.y + A.z * A.z);
		vec3d n;
		n.x = A.x / nMag;
		n.y = A.y / nMag;
		n.z = A.z / nMag;

		return n;
	}

	static float Dot(vec3d A, vec3d B) {
		return A.x * B.x + A.y * B.y + A.z * B.z;
	}
	
private:
	mesh meshCube;
	mat4x4 matProj;
	vec3d vCamera;

	float fTheta;

	void MultMatrixVector(vec3d &i, vec3d &o, mat4x4 &m) {
		o.x = i.x * m.m[0][0] + i.y * m.m[1][0] + i.z * m.m[2][0] + m.m[3][0];
		o.y = i.x * m.m[0][1] + i.y * m.m[1][1] + i.z * m.m[2][1] + m.m[3][1];
		o.z = i.x * m.m[0][2] + i.y * m.m[1][2] + i.z * m.m[2][2] + m.m[3][2];
		float w = i.x * m.m[0][3] + i.y * m.m[1][3] + i.z * m.m[2][3] + m.m[3][3];

		if (w != 0.0f) {
			o.x /= w;
			o.y /= w;
			o.z /= w;

		}
	}

	CHAR_INFO GetColour(float lum)
	{
		short bg_col, fg_col;
		wchar_t sym;
		int pixel_bw = (int)(13.0f*lum);
		switch (pixel_bw)
		{
		case 0: bg_col = BG_BLACK; fg_col = FG_BLACK; sym = PIXEL_SOLID; break;

		case 1: bg_col = BG_BLACK; fg_col = FG_DARK_GREY; sym = PIXEL_QUARTER; break;
		case 2: bg_col = BG_BLACK; fg_col = FG_DARK_GREY; sym = PIXEL_HALF; break;
		case 3: bg_col = BG_BLACK; fg_col = FG_DARK_GREY; sym = PIXEL_THREEQUARTERS; break;
		case 4: bg_col = BG_BLACK; fg_col = FG_DARK_GREY; sym = PIXEL_SOLID; break;

		case 5: bg_col = BG_DARK_GREY; fg_col = FG_GREY; sym = PIXEL_QUARTER; break;
		case 6: bg_col = BG_DARK_GREY; fg_col = FG_GREY; sym = PIXEL_HALF; break;
		case 7: bg_col = BG_DARK_GREY; fg_col = FG_GREY; sym = PIXEL_THREEQUARTERS; break;
		case 8: bg_col = BG_DARK_GREY; fg_col = FG_GREY; sym = PIXEL_SOLID; break;

		case 9:  bg_col = BG_GREY; fg_col = FG_WHITE; sym = PIXEL_QUARTER; break;
		case 10: bg_col = BG_GREY; fg_col = FG_WHITE; sym = PIXEL_HALF; break;
		case 11: bg_col = BG_GREY; fg_col = FG_WHITE; sym = PIXEL_THREEQUARTERS; break;
		case 12: bg_col = BG_GREY; fg_col = FG_WHITE; sym = PIXEL_SOLID; break;
		default:
			bg_col = BG_BLACK; fg_col = FG_BLACK; sym = PIXEL_SOLID;
		}

		CHAR_INFO c;
		c.Attributes = bg_col | fg_col;
		c.Char.UnicodeChar = sym;
		return c;
	}

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

		meshCube.LoadFromObjectFile("ship.obj");

		//projection matrix
		float fNear = 0.1f;
		float fFar = 1000;
		float fFov = 90;

		float fAspectRatio = (float)ScreenHeight() / (float)ScreenWidth();

		float fFovRad = 1.0f / tanf(fFov * 0.5f / 180.0f * PI);

		matProj.m[0][0] = fAspectRatio * fFovRad;
		matProj.m[1][1] = fFovRad;
		matProj.m[2][2] = fFar / (fFar - fNear);
		matProj.m[3][2] = (-fFar * fNear) / (fFar - fNear);
		matProj.m[2][3] = 1.0f;
		matProj.m[3][3] = 0.0f;




		return true;
	}
	
	bool OnUserUpdate(float fElapsedTime) override {
		float sw = (float)ScreenWidth();
		float sh = (float)ScreenHeight();
		Fill(0, 0, ScreenWidth(), ScreenHeight(), PIXEL_SOLID, BG_BLACK);

		mat4x4 matRotZ, matRotX;
		fTheta += fElapsedTime;

		matRotZ.m[0][0] = cosf(fTheta);
		matRotZ.m[0][1] = sinf(fTheta);
		matRotZ.m[1][0] = -sinf(fTheta);
		matRotZ.m[1][1] = cosf(fTheta);
		matRotZ.m[2][2] = 1;
		matRotZ.m[3][3] = 1;

		matRotX.m[0][0] = 1;
		matRotX.m[1][1] = cosf(fTheta * 0.5f);
		matRotX.m[1][2] = sinf(fTheta* 0.5f);
		matRotX.m[2][1] = -sinf(fTheta* 0.5f);
		matRotX.m[2][2] = cosf(fTheta* 0.5f);
		matRotX.m[3][3] = 1;

		vector<triangle> triangles;
		
		
		for (auto tri : meshCube.tris) {
			triangle triProjected, triTranslated, triRotatedZ, triRotatedZX;

			MultMatrixVector(tri.p[0], triRotatedZ.p[0], matRotZ);
			MultMatrixVector(tri.p[1], triRotatedZ.p[1], matRotZ);
			MultMatrixVector(tri.p[2], triRotatedZ.p[2], matRotZ);

			MultMatrixVector(triRotatedZ.p[0], triRotatedZX.p[0], matRotX);
			MultMatrixVector(triRotatedZ.p[1], triRotatedZX.p[1], matRotX);
			MultMatrixVector(triRotatedZ.p[2], triRotatedZX.p[2], matRotX);



			//translate away from viewpoint
			triTranslated = triRotatedZX;
			triTranslated.p[0].z = triRotatedZX.p[0].z + 8.0f;
			triTranslated.p[1].z = triRotatedZX.p[1].z + 8.0f;
			triTranslated.p[2].z = triRotatedZX.p[2].z + 8.0f;


			vec3d normal, line1, line2;
			line1.x = triTranslated.p[1].x - triTranslated.p[0].x;
			line1.y = triTranslated.p[1].y - triTranslated.p[0].y;
			line1.z = triTranslated.p[1].z - triTranslated.p[0].z;

			line2.x = triTranslated.p[2].x - triTranslated.p[0].x;
			line2.y = triTranslated.p[2].y - triTranslated.p[0].y;
			line2.z = triTranslated.p[2].z - triTranslated.p[0].z;

			normal.x = line1.y * line2.z - line1.z * line2.y;
			normal.y = line1.z * line2.x - line1.x * line2.z;
			normal.z = line1.x * line2.y - line1.y * line2.x;

			normal = GetNormalised(normal);
			/*float nMag = sqrt(normal.x * normal.x + normal.y * normal.y + normal.z * normal.z);
			normal.x /= nMag;
			normal.y /= nMag;
			normal.z /= nMag;*/


			//if (normal.z < 0) 
			if (normal.x * (triTranslated.p[0].x - vCamera.x) +
				normal.y * (triTranslated.p[0].y - vCamera.y) +
				normal.z * (triTranslated.p[0].z - vCamera.z) < 0.0f){


				vec3d directionalLight = { 0.0f, 0.0f, -1.0f };
				directionalLight = GetNormalised(directionalLight);

				float dp = Dot(normal, directionalLight);
				CHAR_INFO c = GetColour(dp);
				triTranslated.col = c.Attributes;
				triTranslated.sym = c.Char.UnicodeChar;

				//project
				MultMatrixVector(triTranslated.p[0], triProjected.p[0], matProj);
				MultMatrixVector(triTranslated.p[1], triProjected.p[1], matProj);
				MultMatrixVector(triTranslated.p[2], triProjected.p[2], matProj);
				triProjected.col = triTranslated.col;
				triProjected.sym = triTranslated.sym;

				// Scale into view
				triProjected.p[0].x += 1.0f;
				triProjected.p[0].y += 1.0f;

				triProjected.p[1].x += 1.0f;
				triProjected.p[1].y += 1.0f;

				triProjected.p[2].x += 1.0f;
				triProjected.p[2].y += 1.0f;

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
			float z1 = (t1.p[0].z)
		});



		for (auto &triProjected : triangles) {
			FillTriangle(triProjected.p[0].x, triProjected.p[0].y,
				triProjected.p[1].x, triProjected.p[1].y,
				triProjected.p[2].x, triProjected.p[2].y,
				triProjected.sym, triProjected.col);			
		}

		return true;
	}
};


int main() {
	olcEngine3D demo;

	if (demo.ConstructConsole(256, 240, 4, 4)) {
		demo.Start();
	}	else {
		
	}

	return 0;
}

