namespace ComicReader.Helper
{
	public static class StreamExtensions
	{
		public static readonly byte[] TempArray = new byte[4];

		/// <summary>
		/// Converts stream to byte array.
		/// </summary>
		/// <param name="stream">Stream</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Stream data as array</returns>
		/// <returns>Binary data from stream in an array</returns>
		public static async Task<byte[]> ToArrayAsync(this Stream stream, CancellationToken cancellationToken)
		{
			if (!stream.CanRead) {
				throw new AccessViolationException("Stream cannot be read");
			}

			if (stream.CanSeek) {
				return await ToArrayAsyncDirect(stream, cancellationToken);
			} else {
				return await ToArrayAsyncGeneral(stream, cancellationToken);
			}
		}

		/// <summary>
		/// Converts stream to byte array through MemoryStream. This doubles allocations compared to ToArrayAsyncDirect.
		/// </summary>
		/// <param name="stream">Stream</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		private static async Task<byte[]> ToArrayAsyncGeneral(Stream stream, CancellationToken cancellationToken)
		{
			using MemoryStream memoryStream = new MemoryStream();
			await stream.CopyToAsync(memoryStream, cancellationToken);
			return memoryStream.ToArray();
		}

		/// <summary>
		/// Converts stream to byte array without unnecessary allocations.
		/// </summary>
		/// <param name="stream">Stream</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Stream data as array</returns>
		/// <exception cref="ArgumentException">Thrown if stream is not providing correct Length</exception>
		private static async Task<byte[]> ToArrayAsyncDirect(Stream stream, CancellationToken cancellationToken)
		{
			if (stream.Position > 0) {
				throw new ArgumentException("Stream is not at the start!");
			}


			var array = new byte[stream.Length];
			int bytesRead = await stream.ReadAsync(array, 0, (int)stream.Length, cancellationToken);

			if (bytesRead != array.Length ||
				await stream.ReadAsync(TempArray, 0, TempArray.Length, cancellationToken) > 0) {
				throw new ArgumentException("Stream does not have reliable Length!");
			}

			return array;
		}
	}
}
