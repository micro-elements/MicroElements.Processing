// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using MicroElements.Functional;

namespace MicroElements.Processing.TaskManager
{
    /// <summary>
    /// Represents operation id.
    /// Can not hold null.
    /// </summary>
    public readonly struct OperationId : IEquatable<OperationId>
    {
        private readonly string _value;

        /// <summary>
        /// Gets value.
        /// </summary>
        public string Value => _value ?? throw new NotInitializedException($"{nameof(OperationId)} was not initialized.");

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationId"/> struct.
        /// </summary>
        /// <param name="value">Not null value.</param>
        public OperationId([NotNull] string value) => _value = value.AssertArgumentNotNull(nameof(value));

        /// <inheritdoc />
        public override string ToString() => Value;

        /// <inheritdoc />
        public bool Equals(OperationId other) => _value == other._value;

        /// <summary>
        /// Implicit conversion from string.
        /// </summary>
        /// <param name="value">Source string.</param>
        public static implicit operator OperationId(string value) => new OperationId(value);
    }
}
