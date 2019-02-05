using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;


namespace Epicture
{
    class ImgurImages
    {
        [JsonProperty(PropertyName = "data")]
        public List<ImgurImage> images;
    }

    class ImgurImage
    {
        public String Id;
        public String Description;
        public bool animated;
        private String title;
        private bool favorite;
        private String link;
        private String mp4;
        private String cover;

        public String Title
        {
            get { return title; }
            set
            {
                if (title != null)
                    return;
                if (value != title)
                {
                    title = value;
                    NotifyPropertyChanged("Title");
                }
            }
        }
        public bool Favorite
        {
            get { return favorite; }
            set
            {
                if (value != favorite)
                {
                    favorite = value;
                    NotifyPropertyChanged("Favorite");
                }
            }
        }
        public String Link
        {
            get { return link; }
            set
            {
                if (link != null)
                    return;
                if (value != link)
                {
                    link = value;
                    NotifyPropertyChanged("Link");
                }
            }
        }
        public String Mp4
        {
            get { return mp4; }
            set
            {
                if (mp4 != null)
                    return;
                if (value != mp4)
                {
                    mp4 = value;
                    NotifyPropertyChanged("Mp4");
                }
            }
        }
        public String Cover
        {
            get { return cover; }
            set
            {
                if (value != cover)
                {
                    cover = value;
                    Link = "https://i.imgur.com/" + cover + ".png";
                    NotifyPropertyChanged("Cover");
                }
            }
        }

        public Visibility IsGif
        {
            get
            {
                if (animated)
                {
                    return Visibility.Visible;
                }
                else
                    return Visibility.Collapsed;
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ImgurImage()
        {
        }
    }
}
