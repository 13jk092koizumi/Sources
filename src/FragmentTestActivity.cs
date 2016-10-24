using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace GetWifi.src {
    [Activity(Label = "FragmentTestActivity")]
    public class FragmentTestActivity : Activity {
        protected override void OnCreate(Bundle savedInstanceState) {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.FragmentTest);

            var button = FindViewById<Button>(Resource.Id.new_fragments);
            button.Click += delegate {
                //update();
            };

            //DateTime time = new DateTime();

            /*FragmentTransaction xact = FragmentManager.BeginTransaction();
            if (null == FragmentManager.FindFragmentByTag("FRAG1_TAG")) {
                xact.Add(Resource.Id.date_time, DateFragment.newInstance(time), "FRAG1_TAG");
            }
            if (null == FragmentManager.FindFragmentByTag("FRAG2_TAG")) {
                xact.Add(Resource.Id.date_time2, DateFragment.newInstance(time), "FRAG2_TAG");
            }*/
        }
    }
}