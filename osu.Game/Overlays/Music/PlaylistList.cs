﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.Music
{
    internal class PlaylistList : Container
    {
        private readonly FillFlowContainer<PlaylistItem> items;

        public IEnumerable<BeatmapSetInfo> BeatmapSets
        {
            set
            {
                items.Children = value.Select((item, index) => new PlaylistItem(items, item) { OnSelect = itemSelected, Depth = index }).ToList();
            }
        }

        public BeatmapSetInfo FirstVisibleSet => items.Children.FirstOrDefault(i => i.MatchingFilter)?.BeatmapSetInfo;

        private void itemSelected(BeatmapSetInfo b)
        {
            OnSelect?.Invoke(b);
        }

        public Action<BeatmapSetInfo> OnSelect;

        private readonly SearchContainer search;

        public void Filter(string searchTerm) => search.SearchTerm = searchTerm;

        public BeatmapSetInfo SelectedItem
        {
            get { return items.Children.FirstOrDefault(i => i.Selected)?.BeatmapSetInfo; }
            set
            {
                foreach (PlaylistItem s in items.Children)
                    s.Selected = s.BeatmapSetInfo.ID == value?.ID;
            }
        }

        public PlaylistList()
        {
            Children = new Drawable[]
            {
                new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        search = new SearchContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                items = new ItemSearchContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                },
                            }
                        }
                    },
                },
            };
        }

        public void AddBeatmapSet(BeatmapSetInfo beatmapSet)
        {
            items.Add(new PlaylistItem(items, beatmapSet) { OnSelect = itemSelected, Depth = items.Count });
        }

        public void RemoveBeatmapSet(BeatmapSetInfo beatmapSet)
        {
            PlaylistItem itemToRemove = items.Children.FirstOrDefault(item => item.BeatmapSetInfo.ID == beatmapSet.ID);
            if (itemToRemove != null) items.Remove(itemToRemove);
        }

        private class ItemSearchContainer : FillFlowContainer<PlaylistItem>, IHasFilterableChildren
        {
            public string[] FilterTerms => new string[] { };
            public bool MatchingFilter
            {
                set
                {
                    if (value)
                        InvalidateLayout();
                }
            }

            // Compare with reversed ChildID and Depth
            protected override int Compare(Drawable x, Drawable y) => base.Compare(y, x);

            public IEnumerable<IFilterable> FilterableChildren => Children;

            public ItemSearchContainer()
            {
                LayoutDuration = 200;
                LayoutEasing = Easing.OutQuint;
            }
        }
    }
}