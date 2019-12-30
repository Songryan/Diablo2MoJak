﻿using System.Collections.Generic;
using Diablerie.Engine.Entities;
using Diablerie.Engine.UI;
using Diablerie.Engine.Utility;
using Diablerie.Game.UI;
using UnityEngine;

namespace Diablerie.Engine
{
    class MouseSelection : MonoBehaviour
    {
        private static readonly Vector3 Expand = new Vector3(25, 20) / Iso.pixelsPerUnit;

        public static MouseSelection instance;
        
        private Entity hotEntity;
        private Entity newHotEntity;
        private Entity previous;
        private Vector3 mousePos;
        private Vector3 currentPosition;
        private bool highlightItems;
        private PickupHighlighter pickupHighlighter;
        private readonly HashSet<Pickup> pickups = new HashSet<Pickup>();

        void Awake()
        {
            instance = this;
            pickupHighlighter = new PickupHighlighter();
        }

        void Update()
        {
            bool updateHotEntity = !PlayerController.instance.FixedSelection();
            if (!highlightItems)
                pickups.Clear();
            pickupHighlighter.Show(pickups, updateHotEntity);
            pickups.Clear();
            if (updateHotEntity)
            {
                if (highlightItems && pickupHighlighter.Hot != null)
                    hotEntity = pickupHighlighter.Hot;
                else
                    hotEntity = newHotEntity;
            }
            newHotEntity = null;

            if (hotEntity != null && !highlightItems)
            {
                var character = hotEntity.GetComponent<Character>();
                if (character && character.monStat != null)
                {
                    if (character.monStat.interact)
                    {
                        ShowLabel();
                    }
                    else if (character.monStat.killable)
                    {
                        ShowEnemyBar(character);
                    }
                    else
                    {
                        ShowNothing();
                    }
                }
                else
                {
                    ShowLabel();
                }
            }
            else
            {
                ShowNothing();
            }

            if (previous != null)
            {
                previous.selected = false;
            }
            if (hotEntity != null)
            {
                hotEntity.selected = true;
            }
            previous = hotEntity;
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
        }

        public Entity HotEntity => hotEntity;

        public void SetHighlightItems(bool highlightItems)
        {
            this.highlightItems = highlightItems;
        }

        private void ShowLabel()
        {
            EnemyBar.instance.character = null;
            var labelPosition = hotEntity.transform.position + (Vector3) hotEntity.titleOffset / Iso.pixelsPerUnit;
            Ui.ShowLabel(labelPosition, hotEntity.title);
        }

        private void ShowEnemyBar(Character character)
        {
            EnemyBar.instance.character = character;
            Ui.HideLabel();
        }

        private void ShowNothing()
        {
            EnemyBar.instance.character = null;
            Ui.HideLabel();
        }

        public void Submit(Entity entity)
        {
            if (entity == PlayerController.instance.character)
                return;

            if (entity is Pickup pickup)
                pickups.Add(pickup);

            Bounds bounds = entity.bounds;

            if (PlayerController.instance.FixedSelection())
            {
                if (entity == newHotEntity)
                {
                    currentPosition = bounds.center;
                }
                return;
            }

            bounds.Expand(Expand);
            if (!bounds.Contains(mousePos))
                return;

            bool betterMatch = newHotEntity == null || Tools.ManhattanDistance(mousePos, bounds.center) < Tools.ManhattanDistance(mousePos, currentPosition);
            if (betterMatch)
            {
                newHotEntity = entity;
                currentPosition = bounds.center;
            }
        }
    }
}
