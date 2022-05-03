using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = System.Random;


public class CalculatePIScript : MonoBehaviour
{
    [Header( "PI" )]
    public double EditorCurrentPIValue;
    public float ClosenessToPIResult;
    public decimal CurrentPIValue;
    public int frameCount;
    public int batchSize = 1000;
    
    public uint totalSampleCount;
    public uint insideCircleCount;

    [Header( "Draw Sample Points" )]
    public int TextureResolution = 1000;
    public SpriteRenderer SampleSprite;
    public Color InsideCircleColor;
    public Color OutsideCircleColor;

    [Header( "Canvas" )]
    public TextMeshPro CurrentPIValueText;
    public TextMeshPro SampleCountText;
    public TextMeshPro InCircleSampleCountText;
    public TextMeshPro RatioResultText;
    public TextMeshPro ClosenessText;

    [Header( "Audio" )]
    public AudioClip SampleAudioClip;
    public AudioSource SampleAudioSource;

    
    private Texture2D SampleTexture;
    private Random rng;

    private float startWait = 2F;


    //TODO IMPROVEMENTS
    //switch to big integers
    //use compute shaders or multithread

    void Start()
    {
        SampleTexture = new Texture2D( TextureResolution, TextureResolution );
        SampleTexture.filterMode = FilterMode.Point;

        //Fill transparent
        for( int x = 0; x < SampleTexture.width; x++ )
        {
            for( int y = 0; y < SampleTexture.width; y++ )
            {
                SampleTexture.SetPixel( x,y,new Color( 0,0,0,0 ) );
            }
        }
        SampleTexture.Apply();
        
        SampleSprite.sprite = Sprite.Create( SampleTexture,new Rect(0,0,SampleTexture.width,SampleTexture.height ),Vector2.zero,TextureResolution );
        SampleTexture = SampleSprite.sprite.texture;

        rng = new Random();
    }
    
    void Update()
    {
        if( startWait > 0F )
        {
            startWait -= Time.deltaTime;
        }
        else
        {
            frameCount++;
            SamplePoint();
        }

        
        //Update canvas
        CurrentPIValueText.text = EditorCurrentPIValue.ToString();
        string batchSizeStr = batchSize.ToString().Remove(0);
        
        InCircleSampleCountText.text = "In Circle: " + (insideCircleCount / 1000).ToString() + "k"; 
        SampleCountText.text = "Total: " + (totalSampleCount / 1000).ToString() + "k"; 
        RatioResultText.text = ( (double)insideCircleCount / (double)totalSampleCount ).ToString("F5") + "...";
        ClosenessText.text = "%" + (ClosenessToPIResult*100).ToString("F5");
    }
    

    void SamplePoint()
    {
        SamplePointData[] samplePoints = new SamplePointData[ batchSize ];
        
        //Redefine resolution for better randomness
        rng = new Random(Environment.TickCount * DateTime.Now.Millisecond);
        for( int i = 0; i < batchSize; i++ )
        {
            bool isItInside = false;

            decimal x = (decimal)((rng.NextDouble() * 2.0) - 1.0);
            decimal y = (decimal)((rng.NextDouble() * 2.0) - 1.0);
            
            /*decimal x = (decimal)(UnityEngine.Random.Range(-1F,1F));
            decimal y = (decimal)(UnityEngine.Random.Range(-1F,1F));*/

            decimal distToCenter = x * x + y * y;
            if( distToCenter <= 1M )
            {
                isItInside = true;
                insideCircleCount++;
            }
            
            totalSampleCount++;

            samplePoints[ i ] = new SamplePointData( (float)x, (float)y, isItInside );
        }
        
        CurrentPIValue = 4M * ( (decimal)insideCircleCount / (decimal)totalSampleCount );

        EditorCurrentPIValue =  Decimal.ToDouble( CurrentPIValue);
        ClosenessToPIResult = 1F - (Mathf.Abs( Mathf.PI - (float)EditorCurrentPIValue ) / Mathf.PI);
        DrawSamplePoints( samplePoints );
        
        if(frameCount%20 == 0)
            PlaySampleAudio();
    }

    void DrawSamplePoints(SamplePointData[] newSamplePoints)
    {
        double texResMult = (double)TextureResolution;
        for( int i = 0; i < newSamplePoints.Length; i++ )
        {
            int texCoordX = (int)(RemapToTextureCoord(newSamplePoints[i].realX) * texResMult);
            int texCoordY = (int)(RemapToTextureCoord(newSamplePoints[i].realY) * texResMult);
            Color c = newSamplePoints[i].isInsideCircle ? InsideCircleColor : OutsideCircleColor;
            SampleTexture.SetPixel( texCoordX,texCoordY, c);
        }
        
        SampleTexture.Apply();
    }

    double RemapToTextureCoord(double val)
    {
        return ( 1.0 + val) * 0.5;
    }

    void PlaySampleAudio()
    {
        float closeness = (ClosenessToPIResult - 0.99F) * 100F;
        
        //closeness *= (Mathf.PI - 2F);
        SampleAudioSource.pitch = closeness;
        SampleAudioSource.PlayOneShot(SampleAudioClip,Mathf.Clamp01(closeness));
    }
    
    struct SamplePointData
    {
        public float realX;
        public float realY;
        public bool isInsideCircle;

        public SamplePointData( float realX, float realY, bool isInsideCircle )
        {
            this.realX = realX;
            this.realY = realY;
            this.isInsideCircle = isInsideCircle;
        }
    }
}
