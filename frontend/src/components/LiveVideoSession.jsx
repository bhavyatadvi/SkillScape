import React, { useEffect, useRef, useState } from 'react';
import { useAuth } from '../context/AuthContext';
import * as signalR from '@microsoft/signalr';
import { getAuthToken } from '../services/api';
import { Video, Mic, MicOff, VideoOff, MonitorUp, PhoneOff, Users, Maximize, Minimize } from 'lucide-react';
import { Button } from './ui/button';
import Peer from 'simple-peer/simplepeer.min.js'; // Ensure we use the minified version for browser compatibility in vite sometimes

export const LiveVideoSession = ({ circleId, onClose }) => {
  const { user } = useAuth();
  const [connection, setConnection] = useState(null);
  const [peers, setPeers] = useState({});
  const [stream, setStream] = useState(null);
  const [isMuted, setIsMuted] = useState(false);
  const [isVideoOff, setIsVideoOff] = useState(false);
  const [isScreenSharing, setIsScreenSharing] = useState(false);
  const [maximizedPeer, setMaximizedPeer] = useState(null);
  
  const myVideo = useRef();
  const peersRef = useRef({});
  const screenTrackRef = useRef(null);
  
  useEffect(() => {
    let localStream;
    let newConnection;
    
    navigator.mediaDevices.getUserMedia({ video: true, audio: true }).then((mediaStream) => {
      setStream(mediaStream);
      if (myVideo.current) {
        myVideo.current.srcObject = mediaStream;
      }
      localStream = mediaStream;
      
      const token = getAuthToken();
      const chatHubUrl = import.meta.env.VITE_API_URL
        ? import.meta.env.VITE_API_URL.replace('/api', '/hubs/chat')
        : '/hubs/chat';

      newConnection = new signalR.HubConnectionBuilder()
        .withUrl(chatHubUrl, { accessTokenFactory: () => token })
        .withAutomaticReconnect()
        .build();

      setConnection(newConnection);
      
      newConnection.start().then(() => {
        newConnection.invoke("JoinVideoRoom", circleId);
        
        // When someone else joins, we initiate the call
        newConnection.on("UserJoinedVideo", (connId, userId, userName) => {
          if (connId === newConnection.connectionId) return;
          
          const peer = new Peer({
            initiator: true,
            trickle: true,
            stream: localStream,
            config: { iceServers: [{ urls: 'stun:stun.l.google.com:19302' }, { urls: 'stun:global.stun.twilio.com:3478' }] }
          });

          peer.on('signal', signal => {
            const payload = { ...signal, senderName: user.fullName };
            newConnection.invoke("SendWebRTCSignal", connId, JSON.stringify(payload));
          });

          peer.on('stream', remoteStream => {
            setPeers(prev => ({ ...prev, [connId]: { ...prev[connId], stream: remoteStream } }));
          });
          
          peer.on('close', () => {
             setPeers(prev => {
                const newPeers = { ...prev };
                delete newPeers[connId];
                return newPeers;
             });
          });
          
          peer.on('error', err => console.log('Peer error', err));

          peersRef.current[connId] = peer;
          setPeers(prev => ({ ...prev, [connId]: { peer, userName } }));
        });

        // When receiving signaling data (offer, answer, or ICE candidate)
        newConnection.on("ReceiveWebRTCSignal", (senderConnId, signalDataStr) => {
          const signalData = JSON.parse(signalDataStr);
          
          if (!peersRef.current[senderConnId]) {
            // We are receiving an offer (we didn't initiate)
            const peer = new Peer({
              initiator: false,
              trickle: true,
              stream: localStream,
              config: { iceServers: [{ urls: 'stun:stun.l.google.com:19302' }, { urls: 'stun:global.stun.twilio.com:3478' }] }
            });

            peer.on('signal', signal => {
              const payload = { ...signal, senderName: user.fullName };
              newConnection.invoke("SendWebRTCSignal", senderConnId, JSON.stringify(payload));
            });

            peer.on('stream', remoteStream => {
              setPeers(prev => ({ ...prev, [senderConnId]: { ...prev[senderConnId], stream: remoteStream } }));
            });
            
            peer.on('close', () => {
               setPeers(prev => {
                  const newPeers = { ...prev };
                  delete newPeers[senderConnId];
                  return newPeers;
               });
            });
            
            peer.on('error', err => console.log('Peer error', err));

            peer.signal(signalData);
            peersRef.current[senderConnId] = peer;
            setPeers(prev => ({ ...prev, [senderConnId]: { peer, userName: signalData.senderName || 'Member' } }));
          } else {
            // We already have this peer, just pass the signaling data
            peersRef.current[senderConnId].signal(signalData);
            if (signalData.senderName) {
                setPeers(prev => ({ ...prev, [senderConnId]: { ...prev[senderConnId], userName: signalData.senderName } }));
            }
          }
        });

        newConnection.on("UserLeftVideo", (connId) => {
          if (peersRef.current[connId]) {
            peersRef.current[connId].destroy();
            delete peersRef.current[connId];
            setPeers(prev => {
              const newPeers = { ...prev };
              delete newPeers[connId];
              return newPeers;
            });
          }
        });

        newConnection.on("SessionEnded", () => {
          onClose();
        });
      }).catch(e => console.error("SignalR Connection Error", e));
    });

    return () => {
      if (localStream) {
        localStream.getTracks().forEach(t => t.stop());
      }
      if (newConnection) {
        newConnection.invoke("LeaveVideoRoom", circleId).catch(() => {});
        newConnection.stop();
      }
      Object.values(peersRef.current).forEach(p => p.destroy());
    };
  }, [circleId]);

  const toggleMute = () => {
    if (stream) {
      stream.getAudioTracks()[0].enabled = isMuted;
      setIsMuted(!isMuted);
    }
  };

  const toggleVideo = () => {
    if (stream) {
      stream.getVideoTracks()[0].enabled = isVideoOff;
      setIsVideoOff(!isVideoOff);
    }
  };

  const toggleScreenShare = async () => {
    if (!isScreenSharing) {
      try {
        const screenStream = await navigator.mediaDevices.getDisplayMedia({ cursor: true });
        const screenTrack = screenStream.getVideoTracks()[0];
        screenTrackRef.current = screenTrack;
        
        if (myVideo.current) myVideo.current.srcObject = screenStream;

        Object.values(peersRef.current).forEach(peer => {
          if (stream) {
            const videoTrack = stream.getVideoTracks()[0];
            peer.replaceTrack(videoTrack, screenTrack, stream);
          }
        });

        screenTrack.onended = () => stopScreenShare();
        setIsScreenSharing(true);
      } catch (err) {
        console.error("Error sharing screen", err);
      }
    } else {
      stopScreenShare();
    }
  };

  const stopScreenShare = () => {
    if (myVideo.current && stream) myVideo.current.srcObject = stream;

    if (stream && screenTrackRef.current) {
      const videoTrack = stream.getVideoTracks()[0];
      Object.values(peersRef.current).forEach(peer => {
        try {
            peer.replaceTrack(screenTrackRef.current, videoTrack, stream);
        } catch(e) {
            console.error("Failed to replace track back to camera", e);
        }
      });
    }

    if (screenTrackRef.current) {
      screenTrackRef.current.stop();
      screenTrackRef.current = null;
    }
    
    setIsScreenSharing(false);
  };

  return (
    <div className="bg-slate-900 rounded-2xl overflow-hidden shadow-sm border border-slate-700 h-[60vh] flex flex-col relative">
      <div className="p-3 bg-slate-800 flex justify-between items-center text-white border-b border-slate-700 z-10">
        <h2 className="font-bold flex items-center text-sm">
          <Video className="w-4 h-4 mr-2 text-red-500 animate-pulse" /> Native Collaboration
        </h2>
        <div className="flex items-center gap-4">
          <div className="flex items-center text-xs font-medium bg-slate-700 px-3 py-1 rounded-full">
            <Users className="w-3 h-3 mr-2 text-slate-300" />
            {Object.keys(peers).length + 1} Participant{Object.keys(peers).length + 1 !== 1 ? 's' : ''}
          </div>
          <Button onClick={onClose} variant="ghost" size="sm" className="h-8 text-xs hover:bg-red-500/20 hover:text-red-400">Leave Session</Button>
        </div>
      </div>
      
      <div className={`flex-1 p-4 overflow-y-auto bg-slate-900 grid gap-4 ${
        maximizedPeer ? 'grid-cols-1 md:grid-cols-4 auto-rows-max' : 'grid-cols-2 md:grid-cols-3 auto-rows-max'
      }`}>
        <div className={`relative rounded-xl overflow-hidden bg-black flex items-center justify-center border border-slate-700 transition-all ${
          maximizedPeer === 'local' 
            ? 'md:col-span-3 md:row-span-6 aspect-video md:aspect-auto h-full min-h-[40vh] order-first' 
            : 'aspect-video ' + (maximizedPeer ? 'order-last md:col-span-1' : '')
        }`}>
          <video playsInline muted ref={myVideo} autoPlay className="w-full h-full object-cover" style={{ transform: isScreenSharing ? 'none' : 'scaleX(-1)' }} />
          <div className="absolute bottom-2 left-2 bg-black/60 px-2 py-1 rounded text-white text-xs font-medium backdrop-blur-sm z-10">
            {user?.fullName} (You)
          </div>
          <Button 
            size="icon" 
            variant="ghost" 
            className="absolute top-2 right-2 h-8 w-8 text-white/70 hover:text-white hover:bg-black/50 z-10"
            onClick={() => setMaximizedPeer(maximizedPeer === 'local' ? null : 'local')}
          >
            {maximizedPeer === 'local' ? <Minimize className="w-4 h-4" /> : <Maximize className="w-4 h-4" />}
          </Button>
        </div>
        
        {Object.entries(peers).map(([peerId, peerData]) => (
          <VideoPlayer 
            key={peerId} 
            stream={peerData.stream} 
            userName={peerData.userName} 
            isMaximized={maximizedPeer === peerId}
            isAnyMaximized={!!maximizedPeer}
            onToggleMaximize={() => setMaximizedPeer(maximizedPeer === peerId ? null : peerId)}
          />
        ))}
      </div>

      <div className="p-4 bg-slate-800/80 backdrop-blur-md border-t border-slate-700 flex justify-center items-center gap-4 z-10">
        <Button onClick={toggleMute} variant={isMuted ? "destructive" : "secondary"} size="icon" className="rounded-full w-12 h-12">
          {isMuted ? <MicOff className="w-5 h-5" /> : <Mic className="w-5 h-5" />}
        </Button>
        <Button onClick={toggleVideo} variant={isVideoOff ? "destructive" : "secondary"} size="icon" className="rounded-full w-12 h-12">
          {isVideoOff ? <VideoOff className="w-5 h-5" /> : <Video className="w-5 h-5" />}
        </Button>
        <Button onClick={toggleScreenShare} variant={isScreenSharing ? "default" : "secondary"} size="icon" className={`rounded-full w-12 h-12 ${isScreenSharing ? 'bg-blue-600 hover:bg-blue-700 text-white' : ''}`}>
          <MonitorUp className="w-5 h-5" />
        </Button>
        <Button onClick={onClose} variant="destructive" size="icon" className="rounded-full w-12 h-12">
          <PhoneOff className="w-5 h-5" />
        </Button>
      </div>
    </div>
  );
};

const VideoPlayer = ({ stream, userName, isMaximized, isAnyMaximized, onToggleMaximize }) => {
  const ref = useRef();
  
  useEffect(() => {
    if (ref.current && stream) {
      ref.current.srcObject = stream;
    }
  }, [stream]);

  return (
    <div className={`relative rounded-xl overflow-hidden bg-black flex items-center justify-center border border-slate-700 transition-all ${
        isMaximized 
          ? 'md:col-span-3 md:row-span-6 aspect-video md:aspect-auto h-full min-h-[40vh] order-first' 
          : 'aspect-video ' + (isAnyMaximized ? 'order-last md:col-span-1' : '')
    }`}>
      {stream ? (
        <video playsInline autoPlay ref={ref} className="w-full h-full object-cover" />
      ) : (
        <div className="w-16 h-16 rounded-full bg-slate-700 flex items-center justify-center text-xl text-white font-bold">
          {userName?.charAt(0) || 'U'}
        </div>
      )}
      <div className="absolute bottom-2 left-2 bg-black/60 px-2 py-1 rounded text-white text-xs font-medium backdrop-blur-sm z-10">
        {userName}
      </div>
      <Button 
        size="icon" 
        variant="ghost" 
        className="absolute top-2 right-2 h-8 w-8 text-white/70 hover:text-white hover:bg-black/50 z-10"
        onClick={onToggleMaximize}
      >
        {isMaximized ? <Minimize className="w-4 h-4" /> : <Maximize className="w-4 h-4" />}
      </Button>
    </div>
  );
};
