import React, { useRef, useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { FileText, Upload, CheckCircle2, AlertCircle, Loader2, X, ArrowLeft } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { careerProfileApi } from '@/services/api/careerProfile';

const ALLOWED_EXTS = ['.pdf', '.docx'];
const MAX_SIZE_MB = 10;

export default function ResumeUpload() {
  const navigate = useNavigate();
  const fileInputRef = useRef(null);

  const [file, setFile] = useState(null);
  const [dragActive, setDragActive] = useState(false);
  const [status, setStatus] = useState('idle'); // idle | extracting | analyzing | success | error
  const [errorMsg, setErrorMsg] = useState('');

  const validateFile = (f) => {
    if (!f) return 'No file selected.';
    const ext = '.' + f.name.split('.').pop()?.toLowerCase();
    if (!ALLOWED_EXTS.includes(ext)) {
      return `Unsupported format. Please upload a PDF or DOCX file.`;
    }
    if (f.size > MAX_SIZE_MB * 1024 * 1024) {
      return `File is too large. Maximum allowed size is ${MAX_SIZE_MB} MB.`;
    }
    return null;
  };

  const handleFile = (f) => {
    const err = validateFile(f);
    if (err) {
      setErrorMsg(err);
      setFile(null);
    } else {
      setErrorMsg('');
      setFile(f);
      setStatus('idle');
    }
  };

  const handleDrag = useCallback((e) => {
    e.preventDefault();
    e.stopPropagation();
    if (e.type === 'dragenter' || e.type === 'dragover') setDragActive(true);
    else if (e.type === 'dragleave') setDragActive(false);
  }, []);

  const handleDrop = useCallback((e) => {
    e.preventDefault();
    setDragActive(false);
    const dropped = e.dataTransfer.files?.[0];
    if (dropped) handleFile(dropped);
  }, []);

  const handleExtract = async () => {
    if (!file) return;
    setStatus('extracting');
    setErrorMsg('');

    try {
      // Stage 1: extracting
      await new Promise((r) => setTimeout(r, 600));
      setStatus('analyzing');

      const res = await careerProfileApi.uploadResume(file);
      const extracted = res.data?.data ?? res.data;

      setStatus('success');
      await new Promise((r) => setTimeout(r, 800));

      // Navigate to editor with prefilled data
      navigate('/career-profile/edit', {
        state: { prefillData: extracted, mode: 'resume' },
      });
    } catch (err) {
      setStatus('error');
      const msg = err.message || 'Unable to extract profile information. Please try again or fill manually.';
      setErrorMsg(msg);
    }
  };

  const statusMessages = {
    extracting: 'Extracting information from your resume...',
    analyzing: 'Analyzing your resume with AI...',
    success: 'Profile extracted successfully!',
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 via-blue-50/30 to-indigo-50/20 pt-24 pb-12 px-4">
      <div className="max-w-2xl mx-auto">

        {/* Header */}
        <div className="mb-8">
          <button
            onClick={() => navigate('/career-profile')}
            className="flex items-center gap-2 text-slate-500 hover:text-slate-800 text-sm mb-5 transition-colors"
          >
            <ArrowLeft className="w-4 h-4" />
            Back to Career Profile
          </button>
          <h1 className="text-3xl font-bold text-slate-900 mb-2">Build Your Career Profile</h1>
          <p className="text-slate-500 text-base">
            Upload your resume and we'll automatically extract your career information.
          </p>
        </div>

        {/* Upload Card */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          className="bg-white rounded-2xl shadow-sm border border-slate-200 p-8"
        >
          {/* Drop Zone */}
          <div
            onDragEnter={handleDrag}
            onDragOver={handleDrag}
            onDragLeave={handleDrag}
            onDrop={handleDrop}
            onClick={() => fileInputRef.current?.click()}
            className={`border-2 border-dashed rounded-xl p-10 flex flex-col items-center justify-center cursor-pointer transition-all duration-200 ${
              dragActive
                ? 'border-blue-500 bg-blue-50'
                : file
                ? 'border-green-400 bg-green-50/50'
                : 'border-slate-200 hover:border-blue-300 hover:bg-blue-50/40'
            }`}
          >
            <input
              ref={fileInputRef}
              type="file"
              accept=".pdf,.docx"
              className="hidden"
              onChange={(e) => handleFile(e.target.files?.[0])}
            />

            <AnimatePresence mode="wait">
              {file ? (
                <motion.div
                  key="file-selected"
                  initial={{ opacity: 0, scale: 0.9 }}
                  animate={{ opacity: 1, scale: 1 }}
                  exit={{ opacity: 0, scale: 0.9 }}
                  className="flex flex-col items-center gap-3"
                >
                  <div className="w-14 h-14 bg-green-100 rounded-full flex items-center justify-center">
                    <FileText className="w-7 h-7 text-green-600" />
                  </div>
                  <div className="text-center">
                    <p className="font-semibold text-slate-800">{file.name}</p>
                    <p className="text-sm text-slate-500 mt-1">
                      {(file.size / 1024 / 1024).toFixed(2)} MB
                    </p>
                  </div>
                  <button
                    onClick={(e) => { e.stopPropagation(); setFile(null); setStatus('idle'); }}
                    className="flex items-center gap-1 text-xs text-red-500 hover:text-red-700 mt-1"
                  >
                    <X className="w-3 h-3" /> Remove
                  </button>
                </motion.div>
              ) : (
                <motion.div
                  key="empty"
                  initial={{ opacity: 0 }}
                  animate={{ opacity: 1 }}
                  exit={{ opacity: 0 }}
                  className="flex flex-col items-center gap-4"
                >
                  <div className="w-16 h-16 bg-blue-50 rounded-full flex items-center justify-center">
                    <Upload className="w-8 h-8 text-blue-400" />
                  </div>
                  <div className="text-center">
                    <p className="text-slate-700 font-medium">Drag & drop your resume</p>
                    <p className="text-slate-400 text-sm mt-1">or</p>
                    <p className="text-blue-600 text-sm font-medium mt-1">Choose Resume</p>
                  </div>
                  <p className="text-xs text-slate-400">PDF or DOCX · Max {MAX_SIZE_MB} MB</p>
                </motion.div>
              )}
            </AnimatePresence>
          </div>

          {/* Error message */}
          <AnimatePresence>
            {errorMsg && (
              <motion.div
                initial={{ opacity: 0, height: 0 }}
                animate={{ opacity: 1, height: 'auto' }}
                exit={{ opacity: 0, height: 0 }}
                className="mt-4 flex items-start gap-2 text-red-600 text-sm bg-red-50 rounded-lg p-3"
              >
                <AlertCircle className="w-4 h-4 mt-0.5 shrink-0" />
                <span>{errorMsg}</span>
              </motion.div>
            )}
          </AnimatePresence>

          {/* Status message */}
          <AnimatePresence>
            {['extracting', 'analyzing', 'success'].includes(status) && (
              <motion.div
                initial={{ opacity: 0, height: 0 }}
                animate={{ opacity: 1, height: 'auto' }}
                exit={{ opacity: 0, height: 0 }}
                className={`mt-4 flex items-center gap-2 text-sm rounded-lg p-3 ${
                  status === 'success'
                    ? 'bg-green-50 text-green-700'
                    : 'bg-blue-50 text-blue-700'
                }`}
              >
                {status === 'success' ? (
                  <CheckCircle2 className="w-4 h-4 shrink-0" />
                ) : (
                  <Loader2 className="w-4 h-4 animate-spin shrink-0" />
                )}
                <span>{statusMessages[status]}</span>
              </motion.div>
            )}
          </AnimatePresence>

          {/* Extract Button */}
          <div className="mt-6 flex flex-col gap-3">
            <Button
              size="lg"
              disabled={!file || ['extracting', 'analyzing', 'success'].includes(status)}
              onClick={handleExtract}
              className="w-full bg-blue-600 hover:bg-blue-700 text-white shadow-md shadow-blue-200 disabled:opacity-50"
            >
              {['extracting', 'analyzing'].includes(status) ? (
                <><Loader2 className="w-4 h-4 mr-2 animate-spin" /> Extracting...</>
              ) : (
                <><FileText className="w-4 h-4 mr-2" /> Extract Profile</>
              )}
            </Button>

            <div className="flex items-center gap-3">
              <div className="flex-1 h-px bg-slate-100" />
              <span className="text-xs text-slate-400">or</span>
              <div className="flex-1 h-px bg-slate-100" />
            </div>

            <Button
              variant="outline"
              size="lg"
              className="w-full"
              onClick={() => navigate('/career-profile/edit', { state: { mode: 'manual' } })}
            >
              Fill Form Manually
            </Button>
          </div>
        </motion.div>

        {/* Info note */}
        <p className="text-center text-xs text-slate-400 mt-5">
          Your resume is only used for extracting career information and is not stored.
        </p>
      </div>
    </div>
  );
}
