﻿using System;
using System.IO;

public class Disposable : IDisposable
{
    public void Dispose() { }
}

        class UsingDeclaration
        {
            public void Disposed_UsingDeclaration()
            {
                using var d = new Disposable(); // Noncompliant FP
                d.Dispose(); // FIXME Non-compliant {{Refactor this code to make sure 'd' is disposed only once.}}
            }
        }

        public class NullCoalescenceAssignment
        {
            public void NullCoalescenceAssignment_Compliant(IDisposable s)
            {
                s ??= new Disposable();
                s.Dispose();
            }

            public void NullCoalescenceAssignment_NonCompliant(IDisposable s)
            {
                using (s ??= new Disposable()) // FIXME Non-compliant
                {
                    s.Dispose();
                }
            }
        }

        public interface IWithDefaultMembers
        {
            void DoDispose()
            {
                var d = new Disposable();
                d.Dispose();
                d.Dispose(); // Noncompliant
            }
        }

        public class LocalStaticFunctions
        {
            public void Method(object arg)
            {
                void LocalFunction()
                {
                    var d = new Disposable();
                    d.Dispose();
                    d.Dispose(); // FN: local functions are not supported
                }

                static void LocalStaticFunction()
                {
                    var d = new Disposable();
                    d.Dispose();
                    d.Dispose(); // FN: local functions are not supported
                }
            }
        }

        public ref struct Struct
        {
            public void Dispose()
            {
            }
        }

        public class Consumer
        {
            public void M1()
            {
                var s = new Struct();

                s.Dispose();
                s.Dispose(); // Noncompliant
            }

            public void M2()
            {
                using var s = new Struct(); // Noncompliant FP

                s.Dispose(); // FIXME Non-compliant
            }
        }
