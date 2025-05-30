using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.UIElements;
using UnityEngine.AI;

public class FieldDivision : MonoBehaviour
{
    public GameObject Field;

    public int field_x = 10; //分割数
    public int field_y = 10;
    public float speed = 200f; //移動速度
    public int point_x = 0; //初期化用
    public int point_y = 0;
    public int odds = 100; //パネル移動確率
    public int dup; //一度行ったパネルへの移動確率減少値
    private int mpoint_x = 0; //移動用
    private int mpoint_y = 0;

    private bool move_sw = false;
    private bool chase_sw = false;

    private float fieldSize_x;
    private float fieldSize_y;

    private float divSize_x;
    private float divSize_y;

    private Vector3[,] F_point;
    private Vector3 latestPos;
    private int[,] weight;
    
    // Start is called before the first frame update
    void Start()
    {
        //フィールドの大きさ取得
        fieldSize_x = Field.transform.localScale.x;
        fieldSize_y = Field.transform.localScale.z;

        Debug.Log(fieldSize_x+","+fieldSize_y);

        Mesh mesh = Field.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;

        // 最小のx座標を持つ頂点を初期化
        Vector3 minXVertex = vertices[0];

        //大小比較により最小（図面での左下端）の座標を格納
        foreach (Vector3 vertex in vertices)
        {
            if (vertex.x <= minXVertex.x && vertex.z <= minXVertex.z)
            {
                minXVertex = vertex;
            }
        }

        Debug.Log("座標"+Field.transform.TransformPoint(minXVertex));

        //分割してみる
        //入れる座標は各分割したパネルの中心
        //分割した座標の格納用配列
        F_point = new Vector3[field_x,field_y];
        //確率計算用
        weight = new int [field_x,field_y];

        //中心点間の大きさ→planeにおいて1スケール毎のサイズは5
        divSize_x = fieldSize_x;
        divSize_y = fieldSize_y;

        Debug.Log(minXVertex.x);

        //座標を中心点に移動
        minXVertex.x += (divSize_x / 2)/5;
        minXVertex.z += (divSize_y / 2)/5;

        F_point[0,0] = Field.transform.TransformPoint(minXVertex);
        Debug.Log("座標("+F_point[0,0]+")");

        //配列にぶち込む&重み初期化
        for(int i=0;i<field_x;i++)
        {
            for(int t=0;t<field_y;t++)
            {
                if(i==0) F_point[i,t].x = F_point[0,0].x;
                else F_point[i,t].x = F_point[i-1,0].x + divSize_x;

                if(t==0) F_point[i,t].z = F_point[0,0].z;
                else F_point[i,t].z = F_point[i,t-1].z + divSize_y;

                F_point[i,t].y = 0.5f;

                weight[i,t] = odds;
            }
        }

        //初期位置設定
        this.transform.position = F_point[point_x,point_y];
        weight[point_x,point_y]--;
        Debug.Log(this.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if(!move_sw) DecDistin();
        MoveFun();
    }

    void MoveFun()
    {
        move_sw = true;

        Vector3 diff = transform.position - latestPos;   //前回からどこに進んだかをベクトルで取得
        latestPos = transform.position;  //前回のPositionの更新

        //ベクトルの大きさが0.01以上の時に向きを変える処理をする
        if (diff.magnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(diff); //向きを変更する
        }

        this.transform.position = Vector3.MoveTowards(transform.position,F_point[mpoint_x,mpoint_y],speed*0.01f);

        if(this.transform.position == F_point[mpoint_x,mpoint_y]) move_sw = false;
    }

    void DecDistin()
    {
        int len0 = F_point.GetLength(0);
        int len1 = F_point.GetLength(1);
        int count = 0; //重みリセット用カウンタ
        bool weight_sw = false;

        //現在地が配列においてどこにあたるか
        for(int i=0;i<len0;i++)
        {
            for(int t=0;t<len1;t++)
            {
                if(this.transform.position.x >= (F_point[i,t].x - (divSize_x/2)) && this.transform.position.x <= (F_point[i,t].x + (divSize_x/2)) && this.transform.position.z >= (F_point[i,t].z - (divSize_y/2)) && this.transform.position.z <= (F_point[i,t].z + (divSize_y/2)))
                {
                    mpoint_x = i;
                    mpoint_y = t;
                }
            }
        }

        Debug.Log(mpoint_x+","+mpoint_y);

        // 隣接パネルへランダムに行き先を設定
        if (mpoint_x == 0 && mpoint_y == 0)
        {
            int weightRight = weight[mpoint_x + 1, mpoint_y];
            int weightUp = weight[mpoint_x, mpoint_y + 1];
            int totalWeight = weightRight + weightUp;
            
            int rand = Random.Range(0, totalWeight);
            if (rand < weightRight) mpoint_x++;
            else mpoint_y++;
        }
        else if (mpoint_x == 0 && mpoint_y == 9)
        {
            int weightRight = weight[mpoint_x + 1, mpoint_y];
            int weightDown = weight[mpoint_x, mpoint_y - 1];
            int totalWeight = weightRight + weightDown;

            int rand = Random.Range(0, totalWeight);
            if (rand < weightRight) mpoint_x++;
            else mpoint_y--;
        }
        else if (mpoint_x == 9 && mpoint_y == 0)
        {
            int weightLeft = weight[mpoint_x - 1, mpoint_y];
            int weightUp = weight[mpoint_x, mpoint_y + 1];
            int totalWeight = weightLeft + weightUp;

            int rand = Random.Range(0, totalWeight);
            if (rand < weightLeft) mpoint_x--;
            else mpoint_y++;
        }
        else if (mpoint_x == 9 && mpoint_y == 9)
        {
            int weightLeft = weight[mpoint_x - 1, mpoint_y];
            int weightDown = weight[mpoint_x, mpoint_y - 1];
            int totalWeight = weightLeft + weightDown;

            int rand = Random.Range(0, totalWeight);
            if (rand < weightLeft) mpoint_x--;
            else mpoint_y--;
        }
        else if (mpoint_x == 0)
        {
            int weightRight = weight[mpoint_x + 1, mpoint_y];
            int weightUp = weight[mpoint_x, mpoint_y + 1];
            int weightDown = weight[mpoint_x, mpoint_y - 1];
            int totalWeight = weightRight + weightUp + weightDown;

            int rand = Random.Range(0, totalWeight);
            if (rand < weightRight) mpoint_x++;
            else if (rand < weightRight + weightUp) mpoint_y++;
            else mpoint_y--;
        }
        else if (mpoint_x == 9)
        {
            int weightLeft = weight[mpoint_x - 1, mpoint_y];
            int weightUp = weight[mpoint_x, mpoint_y + 1];
            int weightDown = weight[mpoint_x, mpoint_y - 1];
            int totalWeight = weightLeft + weightUp + weightDown;

            int rand = Random.Range(0, totalWeight);
            if (rand < weightLeft) mpoint_x--;
            else if (rand < weightLeft + weightUp) mpoint_y++;
            else mpoint_y--;
        }
        else if (mpoint_y == 0)
        {
            int weightRight = weight[mpoint_x + 1, mpoint_y];
            int weightLeft = weight[mpoint_x - 1, mpoint_y];
            int weightUp = weight[mpoint_x, mpoint_y + 1];
            int totalWeight = weightRight + weightLeft + weightUp;

            int rand = Random.Range(0, totalWeight);
            if (rand < weightRight) mpoint_x++;
            else if (rand < weightRight + weightLeft) mpoint_x--;
            else mpoint_y++;
        }
        else if (mpoint_y == 9)
        {
            int weightRight = weight[mpoint_x + 1, mpoint_y];
            int weightLeft = weight[mpoint_x - 1, mpoint_y];
            int weightDown = weight[mpoint_x, mpoint_y - 1];
            int totalWeight = weightRight + weightLeft + weightDown;

            int rand = Random.Range(0, totalWeight);
            if (rand < weightRight) mpoint_x++;
            else if (rand < weightRight + weightLeft) mpoint_x--;
            else mpoint_y--;
        }
        else
        {
            int weightRight = weight[mpoint_x + 1, mpoint_y];
            int weightLeft = weight[mpoint_x - 1, mpoint_y];
            int weightUp = weight[mpoint_x, mpoint_y + 1];
            int weightDown = weight[mpoint_x, mpoint_y - 1];
            int totalWeight = weightRight + weightLeft + weightUp + weightDown;

            int rand = Random.Range(0, totalWeight);
            if (rand < weightRight) mpoint_x++;
            else if (rand < weightRight + weightLeft) mpoint_x--;
            else if (rand < weightRight + weightLeft + weightUp) mpoint_y++;
            else mpoint_y--;
        }

        dup = weight[mpoint_x,mpoint_y] / 2;

        //確率を下げる
        if(weight[mpoint_x,mpoint_y] > 5) weight[mpoint_x,mpoint_y] = weight[mpoint_x,mpoint_y] - dup;
        //確率リセット
        //3つ以上のパネルの重みが8以下になっている時
        for(int i=0;i<field_x;i++)
        {
            for(int t=0;t<field_y;t++)
            {
                if(weight[i,t] <= 3) count++;
                if(count >= 3 && !weight_sw)
                {
                    Debug.Log("確率リセット");
                    weight_sw = true;
                    i=0;
                    t=0;
                }
                if(weight_sw) weight[i,t] = odds;
            }
        }

        Debug.Log(mpoint_x+","+mpoint_y);
    }

}
