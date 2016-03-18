using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenMap
{
    /// <summary>
	/// Represents map provider which provide map sources 
	/// for MultiScaleImage control.
	/// </summary>
	public abstract class TiledProvider : MapProviderBase
    {
		/// <summary>
		/// Identifies the IsTileCachingEnabled dependency property.
		/// </summary>
		public static readonly DependencyProperty IsTileCachingEnabledProperty = DependencyProperty.Register(
			"IsTileCachingEnabled",
			typeof(bool),
			typeof(TiledProvider),
			new PropertyMetadata(false, IsTileCachingEnabledHandler));

		/// <summary>
		/// Identifies the CacheStorage dependency property.
		/// </summary>
		public static readonly DependencyProperty CacheStorageProperty = DependencyProperty.Register(
			"CacheStorage",
			typeof(ICacheStorage),
			typeof(TiledProvider),
			new PropertyMetadata(null, CacheStorageHandler));

		/// <summary>
		/// Identifies the RequestCacheLevel dependency property.
		/// </summary>
		public static readonly DependencyProperty RequestCacheLevelProperty = DependencyProperty.Register(
			"RequestCacheLevel",
			typeof(RequestCacheLevel),
			typeof(TiledProvider),
			new PropertyMetadata(RequestCacheLevel.CacheIfAvailable, RequestCacheLevelHandler));

		/// <summary>
		/// Identifies the RequestCredentials dependency property.
		/// </summary>
		public static readonly DependencyProperty RequestCredentialsProperty = DependencyProperty.Register(
			"RequestCredentials",
			typeof(ICredentials),
			typeof(TiledProvider),
			new PropertyMetadata(RequestCredentialsHandler));

		/// <summary>
		/// Gets or sets the IsTileCachingEnabled property.
		/// </summary>
		public bool IsTileCachingEnabled
		{
			get
			{
				return (bool)this.GetValue(TiledProvider.IsTileCachingEnabledProperty);
			}
			set
			{
				this.SetValue(TiledProvider.IsTileCachingEnabledProperty, value);
			}
		}

		/// <summary>
		/// Gets or sets the CacheStorage property.
		/// </summary>
		public ICacheStorage CacheStorage
		{
			get
			{
				return (ICacheStorage)this.GetValue(TiledProvider.CacheStorageProperty);
			}
			set
			{
				this.SetValue(TiledProvider.CacheStorageProperty, value);
			}
		}

		/// <summary>
		/// Gets or sets request cache level for downloading tiles.
		/// </summary>
		public RequestCacheLevel RequestCacheLevel
		{
			get
			{
				return (RequestCacheLevel)this.GetValue(RequestCacheLevelProperty);
			}
			set
			{
				this.SetValue(RequestCacheLevelProperty, value);
			}
		}

		/// <summary>
		/// Gets or sets credentials for downloading tiles.
		/// </summary>
		public ICredentials RequestCredentials
		{
			get
			{
				return (ICredentials)this.GetValue(RequestCredentialsProperty);
			}
			set
			{
				this.SetValue(RequestCredentialsProperty, value);
			}
		}

		/// <summary>
		/// Occurs when tile caching enabled status is changed.
		/// </summary>
		/// <param name="oldValue">Old value.</param>
		/// <param name="newValue">New value.</param>
		protected void IsTileCachingEnabledChanged(bool oldValue, bool newValue)
		{
			this.OnIsTileCachingEnabledChanged(oldValue, newValue);

			this.CacheStorageChanged();
		}

		/// <summary>
		/// Occurs when cache storage is changed.
		/// </summary>
		protected void CacheStorageChanged()
		{
			foreach (TiledMapSource source in this.MapSources.Values)
			{
				this.InheritCacheStorage(source);
			}
		}

		/// <summary>
		/// Occurs when RequestCacheLevel is changed.
		/// </summary>
		protected void RequestCacheLevelChanged()
		{
			foreach (TiledMapSource source in this.MapSources.Values)
			{
				this.InheritRequestCacheLevel(source);
			}
		}

		/// <summary>
		/// Occurs when RequestCredentials is changed.
		/// </summary>
		protected void RequestCredentialsChanged()
		{
			foreach (TiledMapSource source in this.MapSources.Values)
			{
				this.InheritRequestCredentials(source);
			}
		}

		/// <summary>
		/// Copies the CacheStorage and IsTileCachingEnabled properties to source.
		/// </summary>
		/// <param name="source">TiledMapSource instance.</param>
		protected virtual void InheritCacheStorage(TiledMapSource source)
		{
			source.CacheStorage = this.CacheStorage;
			source.IsTileCachingEnabled = this.IsTileCachingEnabled;
		}

		/// <summary>
		/// Calls when the IsTileCachingEnabled property changed.
		/// </summary>
		/// <param name="oldValue"></param>
		/// <param name="newValue"></param>
		protected virtual void OnIsTileCachingEnabledChanged(bool oldValue, bool newValue)
		{
			if (newValue == true && this.CacheStorage == null)
			{
                string profile = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				if (!string.IsNullOrEmpty(profile))
				{
					profile += "\\";
				}

                this.CacheStorage = null; //new FileSystemCache(profile + "TelerikMapCache\\" + this.GetType().Name);
			}
		}

		/// <summary>
		/// Copies the RequestCacheLevel property to source.
		/// </summary>
		/// <param name="source">TiledMapSource instance.</param>
		protected virtual void InheritRequestCacheLevel(TiledMapSource source)
		{
			source.RequestCacheLevel = this.RequestCacheLevel;
		}

		/// <summary>
		/// Copies the RequestCredentials property to source.
		/// </summary>
		/// <param name="source">TiledMapSource instance.</param>
		protected virtual void InheritRequestCredentials(TiledMapSource source)
		{
			source.RequestCredentials = this.RequestCredentials;
		}

		private static void IsTileCachingEnabledHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TiledProvider provider = d as TiledProvider;
			if (provider != null)
			{
				provider.IsTileCachingEnabledChanged((bool)e.OldValue, (bool)e.NewValue);
			}
		}

		private static void CacheStorageHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TiledProvider provider = d as TiledProvider;
			if (provider != null)
			{
				provider.CacheStorageChanged();
			}
		}

		private static void RequestCacheLevelHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TiledProvider provider = d as TiledProvider;
			if (provider != null)
			{
				provider.RequestCacheLevelChanged();
			}
		}

		private static void RequestCredentialsHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TiledProvider provider = d as TiledProvider;
			if (provider != null)
			{
				provider.RequestCredentialsChanged();
			}
		}
    }
}
