﻿using System;
using System.Collections.Generic;

namespace Limxc.Tools.Pipeline.Context
{
    public class PipeContext<T> where T : class
    {
        private readonly Func<T, T> cloner;

        public T Body { get; }

        public List<PipeContextSnapshot<T>> Snapshots { get; }

        public PipeContext(T body, Func<T, T> cloner)
        {
            Body = body;
            this.cloner = cloner;
            Snapshots = new List<PipeContextSnapshot<T>>();
        }

        public void AddSnapshot(string desc)
        {
            var body = cloner?.Invoke(Body);
            if (body != null)
                Snapshots.Add(new PipeContextSnapshot<T>(body, desc));
        }

        public override string ToString()
        {
            return $"{Body} Snapshots: {Snapshots.Count}";
        }
    }
}