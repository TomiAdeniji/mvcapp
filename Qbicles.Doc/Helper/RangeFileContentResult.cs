﻿using System;
using System.Web;

namespace Qbicles.Doc.Helper
{
    /// <summary>
    /// Sends the contents of a binary file to the range response.
    /// </summary>
    public class RangeFileContentResult : RangeFileResult
    {
        #region Properties

        /// <summary>
        /// Gets the binary content to send to the response.
        /// </summary>
        public byte[] FileContents { get; private set; }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the RangeFileContentResult class.
        /// </summary>
        /// <param name="fileContents">The byte array to send to the response.</param>
        /// <param name="contentType">The content type to use for the response.</param>
        /// <param name="fileName">The file name to use for the response.</param>
        /// <param name="modificationDate">The file modification date to use for the response.</param>
        /**
         * <remarks>
         * The <paramref name="modificationDate"/> parameter is used internally while creating ETag and Last-Modified headers. Those headers might by used by client in order to verify that the same entity is being requested in separated partial requests and for caching purposes. Because of that it is important that the value passed to this parameter is consitant and reflects the actual state of entity during its entire lifetime.
         * </remarks>
         */

        public RangeFileContentResult(byte[] fileContents, string contentType, string fileName, DateTime modificationDate)
            : base(contentType, fileName, modificationDate, fileContents.Length, Int32.MaxValue)
        {
            if (fileContents == null)
                throw new ArgumentNullException("fileContents");

            FileContents = fileContents;
        }

        #endregion Constructor

        #region Methods

        /// <summary>
        /// Writes the entire file to the response.
        /// </summary>
        /// <param name="response">The response from context within which the result is executed.</param>
        protected override void WriteEntireEntity(HttpResponseBase response)
        {
            WriteFileContents(response, 0, FileContents.Length);
        }

        /// <summary>
        /// Writes the file range to the response.
        /// </summary>
        /// <param name="response">The response from context within which the result is executed.</param>
        /// <param name="rangeStartIndex">Range start index</param>
        /// <param name="rangeEndIndex">Range end index</param>
        protected override void WriteEntityRange(HttpResponseBase response, long rangeStartIndex, long rangeEndIndex)
        {
            WriteFileContents(response, (int)rangeStartIndex, (int)(rangeEndIndex - rangeStartIndex) + 1);
        }

        private void WriteFileContents(HttpResponseBase response, int offset, int length)
        {
            bool bufferOutput = response.BufferOutput;

            response.BufferOutput = false;
            response.OutputStream.Write(FileContents, offset, length);

            response.BufferOutput = bufferOutput;
        }

        #endregion Methods
    }
}