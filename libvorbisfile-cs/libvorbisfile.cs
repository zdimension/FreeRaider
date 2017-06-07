using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace libvorbisfile_cs
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate uint read_func(void* ptr, uint size, uint nmemb, void* datasource);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate int seek_func(void* datasource, long offset, int whence);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate int close_func(void* datasource);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate int tell_func(void* datasource);

    public unsafe struct ov_callbacks
    {
        public IntPtr read_func;
        public IntPtr seek_func;
        public IntPtr close_func;
        public IntPtr tell_func;
    }

    public static unsafe partial class libvorbisfile
    {
        public const string vorbisfiledll = "libvorbisfile.dll";

        public const int NOTOPEN = 0;
        public const int PARTOPEN = 1;
        public const int OPENED = 2;
        public const int STREAMSET = 3;
        public const int INITSET = 4;

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_clear(OggVorbis_File* vf);

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_fopen(
        [In] [MarshalAs(UnmanagedType.LPStr)] string path, OggVorbis_File* vf);

        /*[DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_open(FILE* f, OggVorbis_File* vf, byte* initial, long ibytes);*/

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_open_callbacks(void* datasource, OggVorbis_File* vf, byte* initial, long ibytes, ov_callbacks callbacks);

        /*[DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_test(FILE* f, OggVorbis_File* vf, 
        byte* initial, 
        long ibytes 
    );*/

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_test_callbacks(void* datasource, OggVorbis_File* vf, 
        byte* initial, 
        long ibytes, ov_callbacks
        callbacks 
    );

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_test_open(OggVorbis_File* vf);

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern long ov_bitrate(OggVorbis_File* vf, int i);

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern long ov_bitrate_instant(OggVorbis_File* vf);

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern long ov_streams(OggVorbis_File* vf);

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern long ov_seekable(OggVorbis_File* vf);

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern long ov_serialnumber(OggVorbis_File* vf, int i);

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern long ov_raw_total(OggVorbis_File* vf, int i);

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern long ov_pcm_total(OggVorbis_File* vf, int i);

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern double ov_time_total(OggVorbis_File* vf, int i);

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_raw_seek(OggVorbis_File* vf, long pos);

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_pcm_seek(OggVorbis_File* vf, long pos);

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_pcm_seek_page(OggVorbis_File* vf, long pos);

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_time_seek(OggVorbis_File* vf, double pos);

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_time_seek_page(OggVorbis_File* vf, double pos);

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_raw_seek_lap(OggVorbis_File* vf, long pos);

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_pcm_seek_lap(OggVorbis_File* vf, long pos);

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_pcm_seek_page_lap(OggVorbis_File* vf, long pos);

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_time_seek_lap(OggVorbis_File* vf, double pos);

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_time_seek_page_lap(OggVorbis_File* vf, double pos);

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern long ov_raw_tell(OggVorbis_File* vf);

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern long ov_pcm_tell(OggVorbis_File* vf);

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern double ov_time_tell(OggVorbis_File* vf);

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern vorbis_info* ov_info(OggVorbis_File* vf, int link);

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern vorbis_comment* ov_comment(OggVorbis_File* vf, int link);

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern long ov_read_float(OggVorbis_File* vf, float*** pcm_channels, int samples,
            int* bitstream);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void filter_d(float** pcm, int channels, int samples, void* filter_param);

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern long ov_read_filter(OggVorbis_File* vf, byte* buffer, int length,
            int bigendianp, int word, int sgned, int* bitstream,
            [MarshalAs(UnmanagedType.FunctionPtr)]filter_d filter,
            void* filter_param
            );

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern long ov_read(OggVorbis_File* vf, byte* buffer, int length,
            int bigendianp, int word, int sgned, int* bitstream);

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_crosslap(OggVorbis_File* vf1, OggVorbis_File* vf2);

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_halfrate(OggVorbis_File* vf, int flag);

        [DllImport(vorbisfiledll, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ov_halfrate_p(OggVorbis_File* vf);
    }

    public unsafe struct ogg_sync_state
    {
        public byte* data;
        public int storage;
        public int fill;
        public int returned;

        public int unsynced;
        public int headerbytes;
        public int bodybytes;
    }

    public unsafe struct vorbis_info
    {
        public int version;
        public int channels;
        public int rate;

        public int bitrate_upper;
        public int bitrate_nominal;
        public int bitrate_lower;
        public int bitrate_window;

        public void* codec_setup;
    }

    public unsafe struct vorbis_comment
    {
        public char** user_comments;
        public int* comment_lengths;
        public int comments;
        public char* vendor;
    }

    public unsafe struct ogg_stream_state
    {
        public byte* body_data; /* bytes from packet bodies */
        public long body_storage; /* storage elements allocated */
        public long body_fill; /* elements stored; fill mark */
        public long body_returned; /* elements of fill returned */


        public int* lacing_vals; /* The values that will go to the segment table */

        public long* granule_vals; /* granulepos values for headers. Not compact
                             this way, but it is simple coupled to the
                             lacing fifo */

        public long lacing_storage;
        public long lacing_fill;
        public long lacing_packet;
        public long lacing_returned;

        public fixed byte header [282]; /* working space for header encode */
        public int header_fill;

        public int e_o_s; /* set when we have buffered the last packet in the
                                      logical bitstream */

        public int b_o_s; /* set after we've written the initial page
                                      of a logical bitstream */

        public long serialno;
        public int pageno;

        public long packetno; /* sequence number for decode; the framing
                             knows where there's a hole in the data,
                             but we need coupling so that the codec
                             (which is in a seperate abstraction
                             layer) also knows about the gap */

        public long granulepos;
    }

    public unsafe struct vorbis_dsp_state
    {
        public int analysisp;
        public vorbis_info* vi;

        public float** pcm;
        public float** pcmret;
        public int pcm_storage;
        public int pcm_current;
        public int pcm_returned;

        public int preextrapolate;
        public int eofflag;

        public long lW;
        public long W;
        public long nW;
        public long centerW;

        public long granulepos;
        public long sequence;

        public long glue_bits;
        public long time_bits;
        public long floor_bits;
        public long res_bits;

        public void* backend_state;
    }

    public unsafe struct vorbis_block
    {
        /* necessary stream state for linking to the framing abstraction */
        public float** pcm; /* this is a pointer into local storage */
        public oggpack_buffer opb;

        public long lW;
        public long W;
        public long nW;
        public int pcmend;
        public int mode;

        public int eofflag;
        public long granulepos;
        public long sequence;
        public vorbis_dsp_state* vd; /* For read-only access of configuration */

        /* local storage to avoid remallocing; it's up to the mapping to
           structure it */
        public void* localstore;
        public long localtop;
        public long localalloc;
        public long totaluse;
        public alloc_chain* reap;

        /* bitmetrics for the frame */
        public long glue_bits;
        public long time_bits;
        public long floor_bits;
        public long res_bits;

        public void* _internal;
    }

    public unsafe struct alloc_chain
    {
        public void* ptr;
        public alloc_chain* next;
    }

    public unsafe struct oggpack_buffer
    {
        public int endbyte;
        public int endbit;

        public byte* buffer;
        public byte* ptr;
        public int storage;
    }

    public unsafe struct OggVorbis_File
    {
        public void* datasource;
        public int seekable;
        public long offset;
        public long end;
        public ogg_sync_state oy;

        public int links;
        public long* offsets;
        public long* dataoffsets;
        public int* serialnos;
        public long* pcmlengths;

        public vorbis_info* vi;
        public vorbis_comment* vc;

        public long pcm_offset;
        public int ready_state;
        public int current_serialno;
        public int current_link;

        public double bittrack;
        public double samptrack;

        public ogg_stream_state os;

        public vorbis_dsp_state vd;
        public vorbis_block vb;

        public ov_callbacks callbacks;
    }
}
