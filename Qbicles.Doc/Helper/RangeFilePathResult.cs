﻿using System;
using System.Web;

namespace Qbicles.Doc.Helper
{
    /// <summary>
    /// Sends the contents of a file to the range response.
    /// </summary>
    public class RangeFilePathResult : RangeFileResult
    {
        #region Fields

        private const int _bufferSize = 0x1000;

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the RangeFilePathResult class.
        /// </summary>
        /// <param name="contentType">The content type to use for the response.</param>
        /// <param name="fileName">The file name to use for the response.</param>
        /// <param name="modificationDate">The file modification date to use for the response.</param>
        /// <param name="fileLength">The file length to use for the response.</param>
        /**
         * <remarks>
         * The <paramref name="modificationDate"/> parameter is used internally while creating ETag and Last-Modified headers. Those headers might by used by client in order to verify that the same entity is being requested in separated partial requests and for caching purposes. Because of that it is important that the value passed to this parameter is consitant and reflects the actual state of entity during its entire lifetime.
         * </remarks>
         */

        public RangeFilePathResult(string contentType, string fileName, DateTime modificationDate, long fileLength)
            : base(contentType, fileName, modificationDate, fileLength)
        {
            if (String.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName");
        }

        #endregion Constructor

        #region Methods

        /// <summary>
        /// Writes the entire file to the response.
        /// </summary>
        /// <param name="response">The response from context within which the result is executed.</param>
        protected override void WriteEntireEntity(HttpResponseBase response)
        {
            response.TransmitFile(FileName);
        }

        /// <summary>
        /// Writes the file range to the response.
        /// </summary>
        /// <param name="response">The response from context within which the result is executed.</param>
        /// <param name="rangeStartIndex">Range start index</param>
        /// <param name="rangeEndIndex">Range end index</param>
        protected override void WriteEntityRange(HttpResponseBase response, long rangeStartIndex, long rangeEndIndex)
        {
            response.TransmitFile(FileName, rangeStartIndex, (rangeEndIndex - rangeStartIndex) + 1);
        }

        #endregion Methods
    }
}