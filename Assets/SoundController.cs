using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
   private static SoundController _instance;
   public static SoundController Instance => _instance;
   public AudioSource audioSource;
   public AudioClip win;
   public AudioClip danhco;

   private void Start()
   {
      _instance = this;
   }

   public void Danhco()
   {
      audioSource.clip = danhco;
      audioSource.loop = false;
      audioSource.Play();
   }
   
   public void Win()
   {
      audioSource.clip = win;
      audioSource.loop = false;
      audioSource.Play();
   }
   
}
