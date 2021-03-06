﻿/*
Copyright (c) 2015, Alistair Singh
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace tsqlc.Util
{
  public delegate void BenchmarkBegin(string name, int id);
  public delegate void BenchmarkEnd(string name, int id, long ellapseTime);

  public class Benchmark : IDisposable
  {
    [ThreadStatic]
    private static int InstanceID = 0;
    [ThreadStatic]
    private static List<string> Namespace = new List<string> { "*" + Thread.CurrentThread.ManagedThreadId.ToString() };

    public static event BenchmarkBegin Begin;
    public static event BenchmarkEnd End;

    private Stopwatch _watch;
    private string _name;
    private int _id;

    private Benchmark(string name)
    {
      Namespace.Add(name);
      _name = string.Format("{0}", string.Join(".", Namespace));
      _watch = new Stopwatch();
      _id = ++InstanceID;

      if(Begin  != null)
        Begin(_name, _id);

      _watch.Start();
    }

    public static IDisposable Start(string name)
    {
      return new Benchmark(name);
    }

    public void Dispose()
    {
      _watch.Stop();
      if (Namespace.Count > 1)
        Namespace.RemoveAt(Namespace.Count - 1);
      if(End != null)
        End(_name, _id, _watch.ElapsedMilliseconds);
    }
  }
}
