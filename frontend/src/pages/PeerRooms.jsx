import React, { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { toast } from 'sonner';
import {
  CalendarClock,
  Check,
  Circle,
  ClipboardList,
  Loader2,
  Maximize2,
  Mic,
  MicOff,
  MonitorUp,
  PenLine,
  Plus,
  Send,
  Users,
  Video,
  VideoOff,
  X,
} from 'lucide-react';
import { useAuth } from '@/context/AuthContext';
import { getAuthToken, peerRoomsApi } from '@/services/api';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Textarea } from '@/components/ui/textarea';

const emptyForm = {
  title: '',
  description: '',
  collaborationType: 'Study Group',
  maxMembers: 6,
  scheduledAt: '',
  durationMinutes: 60,
};

const initials = (name = 'User') =>
  name
    .split(' ')
    .map((part) => part[0])
    .join('')
    .slice(0, 2)
    .toUpperCase();

const formatSchedule = (value) => {
  if (!value) return 'Flexible';
  return new Date(value + (!value.endsWith('Z') ? 'Z' : '')).toLocaleString([], {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
};

const getMediaErrorMessage = (error, deviceLabel) => {
  if (error?.name === 'NotAllowedError' || error?.name === 'SecurityError') {
    return `${deviceLabel} permission is blocked. Allow it in the browser permission prompt or site settings, then try again.`;
  }

  if (error?.name === 'NotFoundError' || error?.name === 'DevicesNotFoundError') {
    return `No ${deviceLabel.toLowerCase()} device was found on this computer.`;
  }

  if (error?.name === 'NotReadableError' || error?.name === 'TrackStartError') {
    return `${deviceLabel} is already being used by another app. Close that app and try again.`;
  }

  if (error?.name === 'AbortError') {
    return `${deviceLabel} could not be started by the browser. Check Windows privacy settings and close any app using it.`;
  }

  return `Could not start ${deviceLabel.toLowerCase()}${error?.name ? ` (${error.name})` : ''}.`;
};

const VideoTile = ({
  stream,
  streamVersion = 0,
  name,
  muted = false,
  isLocal = false,
  micEnabled = false,
  cameraEnabled = false,
  screenSharing = false,
  onExpand,
}) => {
  const videoRef = useRef(null);

  useEffect(() => {
    if (videoRef.current) {
      videoRef.current.srcObject = null;
      videoRef.current.srcObject = stream || null;
      videoRef.current.play?.().catch(() => {});
    }
  }, [stream, streamVersion]);

  const hasVideo = Boolean(stream?.getVideoTracks().some((track) => track.readyState === 'live'));
  const showVideo = Boolean(stream?.getVideoTracks().some((track) => track.readyState === 'live' && track.enabled));

  return (
    <div className="relative flex aspect-video min-h-36 overflow-hidden rounded-lg border border-white/10 bg-slate-900 text-white">
      {showVideo ? (
        <video ref={videoRef} autoPlay playsInline muted={muted} className="h-full w-full object-cover" />
      ) : (
        <div className="flex h-full w-full items-center justify-center">
          <div className="text-center">
            <div className="mx-auto mb-3 flex h-14 w-14 items-center justify-center rounded-full bg-teal-500 text-lg font-bold">
              {initials(name)}
            </div>
            <p className="text-sm font-medium">{name}</p>
          </div>
        </div>
      )}
      <div className="absolute bottom-2 left-2 flex items-center gap-2 rounded-md bg-black/55 px-2 py-1 text-xs">
        <span className="max-w-36 truncate">{isLocal ? `${name} (You)` : name}</span>
        {!micEnabled && <MicOff className="h-3.5 w-3.5" />}
        {screenSharing && <MonitorUp className="h-3.5 w-3.5" />}
        {!cameraEnabled && !screenSharing && <VideoOff className="h-3.5 w-3.5" />}
      </div>
      {(screenSharing || hasVideo) && stream && onExpand && (
        <Button
          type="button"
          size="icon"
          variant="secondary"
          onClick={onExpand}
          className="absolute right-2 top-2 h-8 w-8 bg-white/90 text-slate-900 hover:bg-white"
          title="Maximize shared screen"
        >
          <Maximize2 className="h-4 w-4" />
        </Button>
      )}
    </div>
  );
};

const SharedScreenOverlay = ({ stream, name, onClose }) => {
  const videoRef = useRef(null);

  useEffect(() => {
    if (videoRef.current) {
      videoRef.current.srcObject = null;
      videoRef.current.srcObject = stream || null;
      videoRef.current.play?.().catch(() => {});
    }
  }, [stream]);

  if (!stream) return null;

  return (
    <div className="fixed inset-0 z-50 flex flex-col bg-slate-950 text-white">
      <div className="flex h-14 items-center justify-between border-b border-white/10 px-4">
        <div className="flex items-center gap-2">
          <MonitorUp className="h-5 w-5 text-teal-300" />
          <span className="font-semibold">{name} is sharing screen</span>
        </div>
        <Button type="button" variant="ghost" size="icon" onClick={onClose} className="text-white hover:bg-white/10 hover:text-white">
          <X className="h-5 w-5" />
        </Button>
      </div>
      <div className="flex min-h-0 flex-1 items-center justify-center p-4">
        <video ref={videoRef} autoPlay playsInline className="max-h-full max-w-full rounded-lg border border-white/10 bg-black object-contain shadow-2xl" />
      </div>
    </div>
  );
};

const PeerRooms = () => {
  const { user } = useAuth();
  const [rooms, setRooms] = useState([]);
  const [selectedRoomId, setSelectedRoomId] = useState(null);
  const [activeRoom, setActiveRoom] = useState(null);
  const [loading, setLoading] = useState(true);
  const [roomLoading, setRoomLoading] = useState(false);
  const [creating, setCreating] = useState(false);
  const [form, setForm] = useState(emptyForm);
  const [showCreate, setShowCreate] = useState(false);
  const [message, setMessage] = useState('');
  const [taskTitle, setTaskTitle] = useState('');
  const [sharedNotes, setSharedNotes] = useState('');
  const [whiteboardState, setWhiteboardState] = useState('');
  const [connection, setConnection] = useState(null);
  const [micEnabled, setMicEnabled] = useState(false);
  const [cameraEnabled, setCameraEnabled] = useState(false);
  const [screenSharing, setScreenSharing] = useState(false);
  const [localStreamVersion, setLocalStreamVersion] = useState(0);
  const [remoteStreams, setRemoteStreams] = useState({});
  const [remoteMediaStatus, setRemoteMediaStatus] = useState({});
  const [remoteStreamVersions, setRemoteStreamVersions] = useState({});
  const [focusedShare, setFocusedShare] = useState(null);
  const messagesEndRef = useRef(null);
  const localStreamRef = useRef(new MediaStream());
  const peerConnectionsRef = useRef({});
  const makingOfferRef = useRef({});
  const ignoreOfferRef = useRef({});
  const connectionRef = useRef(null);
  const selectedRoomIdRef = useRef(null);
  const screenTrackRef = useRef(null);
  const cameraTrackRef = useRef(null);

  const localStream = useMemo(() => localStreamRef.current, [localStreamVersion]);

  const canEnterRoom = activeRoom?.currentUserStatus === 'Approved';
  const approvedParticipants = useMemo(
    () => activeRoom?.participants?.filter((participant) => participant.status === 'Approved') || [],
    [activeRoom]
  );
  const pendingParticipants = useMemo(
    () => activeRoom?.participants?.filter((participant) => participant.status === 'Pending') || [],
    [activeRoom]
  );

  const loadRooms = useCallback(async () => {
    try {
      const res = await peerRoomsApi.getRooms();
      const data = res.data || [];
      setRooms(data);
      if (!selectedRoomId && data.length > 0) {
        setSelectedRoomId(data[0].id);
      }
    } catch (err) {
      toast.error(err.message || 'Failed to load rooms');
    } finally {
      setLoading(false);
    }
  }, [selectedRoomId]);

  const loadRoom = useCallback(async (roomId) => {
    if (!roomId) return;
    try {
      setRoomLoading(true);
      const res = await peerRoomsApi.getRoom(roomId);
      const data = res.data;
      setActiveRoom(data);
      setSharedNotes(data.sharedNotes || '');
      setWhiteboardState(data.whiteboardState || '');
    } catch (err) {
      setActiveRoom(null);
      toast.error(err.status === 403 ? 'Your join request needs creator approval first' : err.message || 'Failed to open room');
    } finally {
      setRoomLoading(false);
    }
  }, []);

  useEffect(() => {
    loadRooms();
  }, [loadRooms]);

  useEffect(() => {
    if (selectedRoomId) {
      loadRoom(selectedRoomId);
    }
  }, [selectedRoomId, loadRoom]);

  useEffect(() => {
    selectedRoomIdRef.current = selectedRoomId;
  }, [selectedRoomId]);

  const sendWebRtcSignal = useCallback(async (signalType, targetId, data) => {
    const liveConnection = connectionRef.current;
    const roomId = selectedRoomIdRef.current;
    if (!liveConnection || !roomId || liveConnection.state !== signalR.HubConnectionState.Connected) {
      return;
    }

    await liveConnection.invoke('SendWebRtcSignal', roomId, signalType, JSON.stringify({ targetId, ...data }));
  }, []);

  const publishMediaStatus = useCallback(async (overrides = {}) => {
    const liveConnection = connectionRef.current;
    const roomId = selectedRoomIdRef.current;
    if (!liveConnection || !roomId || liveConnection.state !== signalR.HubConnectionState.Connected) return;

    await liveConnection.invoke(
      'SendWebRtcSignal',
      roomId,
      'media-status',
      JSON.stringify({
        targetId: null,
        micEnabled,
        cameraEnabled,
        screenSharing,
        ...overrides,
      })
    );
  }, [cameraEnabled, micEnabled, screenSharing]);

  const setRemoteStreamForUser = useCallback((remoteUserId, stream) => {
    setRemoteStreams((current) => ({ ...current, [remoteUserId]: stream }));
    setRemoteStreamVersions((current) => ({ ...current, [remoteUserId]: (current[remoteUserId] || 0) + 1 }));
  }, []);

  const createPeerConnection = useCallback((remoteUserId) => {
    if (peerConnectionsRef.current[remoteUserId]) {
      return peerConnectionsRef.current[remoteUserId];
    }

    const peerConnection = new RTCPeerConnection({
      iceServers: [{ urls: 'stun:stun.l.google.com:19302' }],
    });

    makingOfferRef.current[remoteUserId] = false;
    ignoreOfferRef.current[remoteUserId] = false;

    localStreamRef.current.getTracks().forEach((track) => {
      peerConnection.addTrack(track, localStreamRef.current);
    });

    peerConnection.onnegotiationneeded = async () => {
      try {
        makingOfferRef.current[remoteUserId] = true;
        await peerConnection.setLocalDescription();
        await sendWebRtcSignal('description', remoteUserId, { description: peerConnection.localDescription });
      } catch (error) {
        console.error('WebRTC negotiation failed:', error);
      } finally {
        makingOfferRef.current[remoteUserId] = false;
      }
    };

    peerConnection.onicecandidate = (event) => {
      if (event.candidate) {
        sendWebRtcSignal('ice-candidate', remoteUserId, { candidate: event.candidate });
      }
    };

    peerConnection.ontrack = (event) => {
      const [incomingStream] = event.streams;
      if (incomingStream) {
        setRemoteStreamForUser(remoteUserId, incomingStream);
      }
    };

      peerConnection.onconnectionstatechange = () => {
      if (['failed', 'closed', 'disconnected'].includes(peerConnection.connectionState)) {
        setRemoteStreams((current) => {
          const next = { ...current };
          delete next[remoteUserId];
          return next;
        });
        setRemoteStreamVersions((current) => {
          const next = { ...current };
          delete next[remoteUserId];
          return next;
        });
      }
    };

    peerConnectionsRef.current[remoteUserId] = peerConnection;
    return peerConnection;
  }, [sendWebRtcSignal, setRemoteStreamForUser]);

  const createOfferForUser = useCallback(async (remoteUserId) => {
    if (!remoteUserId || remoteUserId === user?.id) return;

    const peerConnection = createPeerConnection(remoteUserId);
    try {
      makingOfferRef.current[remoteUserId] = true;
      await peerConnection.setLocalDescription();
      await sendWebRtcSignal('description', remoteUserId, { description: peerConnection.localDescription });
    } finally {
      makingOfferRef.current[remoteUserId] = false;
    }
  }, [createPeerConnection, sendWebRtcSignal, user?.id]);

  const replaceOutgoingVideoTrack = useCallback((track) => {
    Object.values(peerConnectionsRef.current).forEach((peerConnection) => {
      const sender = peerConnection.getSenders().find((item) => item.track?.kind === 'video');
      if (sender) {
        sender.replaceTrack(track);
      } else if (track) {
        peerConnection.addTrack(track, localStreamRef.current);
      }
    });
  }, []);

  const replaceOutgoingAudioTrack = useCallback((track) => {
    Object.values(peerConnectionsRef.current).forEach((peerConnection) => {
      const sender = peerConnection.getSenders().find((item) => item.track?.kind === 'audio');
      if (sender) {
        sender.replaceTrack(track);
      } else if (track) {
        peerConnection.addTrack(track, localStreamRef.current);
      }
    });
  }, []);

  const renegotiateWithParticipants = useCallback(() => {
    approvedParticipants
      .filter((participant) => participant.userId !== user?.id)
      .forEach((participant) => {
        createOfferForUser(participant.userId).catch(() => {});
      });
  }, [approvedParticipants, createOfferForUser, user?.id]);

  const stopLocalAudio = useCallback(async () => {
    localStreamRef.current.getAudioTracks().forEach((track) => {
      track.stop();
      localStreamRef.current.removeTrack(track);
    });
    replaceOutgoingAudioTrack(null);
    setMicEnabled(false);
    setLocalStreamVersion((value) => value + 1);
    await publishMediaStatus({ micEnabled: false });
  }, [publishMediaStatus, replaceOutgoingAudioTrack]);

  const startLocalAudio = useCallback(async () => {
    try {
      const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
      const [track] = stream.getAudioTracks();
      if (!track) {
        throw new DOMException('No audio track was returned by the browser.', 'NotFoundError');
      }
      localStreamRef.current.getAudioTracks().forEach((oldTrack) => {
        oldTrack.stop();
        localStreamRef.current.removeTrack(oldTrack);
      });
      localStreamRef.current.addTrack(track);
      replaceOutgoingAudioTrack(track);
      setMicEnabled(true);
      setLocalStreamVersion((value) => value + 1);
      await publishMediaStatus({ micEnabled: true });
      renegotiateWithParticipants();
    } catch (error) {
      toast.error(getMediaErrorMessage(error, 'Microphone'));
    }
  }, [publishMediaStatus, renegotiateWithParticipants, replaceOutgoingAudioTrack]);

  const stopLocalVideo = useCallback(async ({ keepScreen = false } = {}) => {
    if (cameraTrackRef.current) {
      cameraTrackRef.current.onended = null;
      cameraTrackRef.current.stop();
      localStreamRef.current.removeTrack(cameraTrackRef.current);
      cameraTrackRef.current = null;
    }

    if (!keepScreen) {
      replaceOutgoingVideoTrack(null);
    }

    setCameraEnabled(false);
    setLocalStreamVersion((value) => value + 1);
    await publishMediaStatus({ cameraEnabled: false });
  }, [publishMediaStatus, replaceOutgoingVideoTrack]);

  const startLocalVideo = useCallback(async () => {
    try {
      const stream = await navigator.mediaDevices.getUserMedia({ video: true });
      const [track] = stream.getVideoTracks();
      if (!track) {
        throw new DOMException('No video track was returned by the browser.', 'NotFoundError');
      }

      if (cameraTrackRef.current) {
        cameraTrackRef.current.stop();
        localStreamRef.current.removeTrack(cameraTrackRef.current);
      }

      if (screenTrackRef.current) {
        screenTrackRef.current.onended = null;
        screenTrackRef.current.stop();
        localStreamRef.current.removeTrack(screenTrackRef.current);
        screenTrackRef.current = null;
        setScreenSharing(false);
      }

      cameraTrackRef.current = track;
      localStreamRef.current.addTrack(track);
      replaceOutgoingVideoTrack(track);
      setCameraEnabled(true);
      setLocalStreamVersion((value) => value + 1);
      await publishMediaStatus({ cameraEnabled: true, screenSharing: false });
      renegotiateWithParticipants();
    } catch (error) {
      toast.error(getMediaErrorMessage(error, 'Camera'));
    }
  }, [publishMediaStatus, renegotiateWithParticipants, replaceOutgoingVideoTrack]);

  const stopScreenShare = useCallback(async () => {
    if (screenTrackRef.current) {
      screenTrackRef.current.onended = null;
      screenTrackRef.current.stop();
      localStreamRef.current.removeTrack(screenTrackRef.current);
      screenTrackRef.current = null;
    }

    setScreenSharing(false);

    if (cameraEnabled) {
      await startLocalVideo();
    } else {
      replaceOutgoingVideoTrack(null);
      setLocalStreamVersion((value) => value + 1);
      await publishMediaStatus({ screenSharing: false });
    }
  }, [cameraEnabled, publishMediaStatus, replaceOutgoingVideoTrack, startLocalVideo]);

  const startScreenShare = useCallback(async () => {
    try {
      const stream = await navigator.mediaDevices.getDisplayMedia({ video: true, audio: false });
      const [track] = stream.getVideoTracks();
      if (!track) {
        throw new DOMException('No screen track was returned by the browser.', 'NotFoundError');
      }

      if (cameraTrackRef.current) {
        cameraTrackRef.current.onended = null;
        cameraTrackRef.current.stop();
        localStreamRef.current.removeTrack(cameraTrackRef.current);
        cameraTrackRef.current = null;
      }
      if (screenTrackRef.current) {
        screenTrackRef.current.onended = null;
        screenTrackRef.current.stop();
        localStreamRef.current.removeTrack(screenTrackRef.current);
      }

      screenTrackRef.current = track;
      localStreamRef.current.addTrack(track);
      replaceOutgoingVideoTrack(track);
      setScreenSharing(true);
      setCameraEnabled(false);
      setLocalStreamVersion((value) => value + 1);
      await publishMediaStatus({ cameraEnabled: false, screenSharing: true });
      renegotiateWithParticipants();

      track.onended = () => {
        stopScreenShare().catch(() => {});
      };
    } catch (error) {
      toast.error(getMediaErrorMessage(error, 'Screen sharing'));
    }
  }, [publishMediaStatus, renegotiateWithParticipants, replaceOutgoingVideoTrack, stopScreenShare]);

  const handleMicToggle = () => {
    if (!navigator.mediaDevices?.getUserMedia) {
      toast.error('This browser does not support microphone capture');
      return;
    }

    if (micEnabled) {
      stopLocalAudio();
    } else {
      startLocalAudio();
    }
  };

  const handleCameraToggle = () => {
    if (!navigator.mediaDevices?.getUserMedia) {
      toast.error('This browser does not support camera capture');
      return;
    }

    if (cameraEnabled) {
      stopLocalVideo();
    } else {
      startLocalVideo();
    }
  };

  const handleScreenShareToggle = () => {
    if (!navigator.mediaDevices?.getDisplayMedia) {
      toast.error('This browser does not support screen sharing');
      return;
    }

    if (screenSharing) {
      stopScreenShare();
    } else {
      startScreenShare();
    }
  };

  useEffect(() => {
    if (!selectedRoomId || !canEnterRoom) return undefined;

    const token = getAuthToken();
    const hubUrl = import.meta.env.VITE_API_URL
      ? import.meta.env.VITE_API_URL.replace('/api', '/hubs/peer-rooms')
      : '/hubs/peer-rooms';

    const nextConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .build();

    nextConnection.on('ReceiveRoomMessage', (incoming) => {
      setActiveRoom((current) =>
        current ? { ...current, messages: [...(current.messages || []), incoming] } : current
      );
    });

    nextConnection.on('WorkspaceUpdated', (payload) => {
      setSharedNotes(payload.sharedNotes || '');
      setWhiteboardState(payload.whiteboardState || '');
    });

    nextConnection.on('ParticipantOnline', ({ userId }) => {
      if (userId && userId !== user?.id) {
        createOfferForUser(userId).catch(() => {});
      }
    });

    nextConnection.on('ReceiveWebRtcSignal', async ({ senderId, signalType, payload }) => {
      if (!senderId || senderId === user?.id) return;

      const data = JSON.parse(payload || '{}');
      if (data.targetId && data.targetId !== user?.id) return;

      if (signalType === 'media-status') {
        setRemoteMediaStatus((current) => ({
          ...current,
          [senderId]: {
            micEnabled: Boolean(data.micEnabled),
            cameraEnabled: Boolean(data.cameraEnabled),
            screenSharing: Boolean(data.screenSharing),
          },
        }));
        return;
      }

      const peerConnection = createPeerConnection(senderId);

      if (signalType === 'description' && data.description) {
        const description = new RTCSessionDescription(data.description);
        const readyForOffer =
          !makingOfferRef.current[senderId] &&
          (peerConnection.signalingState === 'stable' || description.type === 'answer');
        const offerCollision = description.type === 'offer' && !readyForOffer;
        const polite = String(user?.id || '') > String(senderId);

        ignoreOfferRef.current[senderId] = !polite && offerCollision;
        if (ignoreOfferRef.current[senderId]) return;

        await peerConnection.setRemoteDescription(description);

        if (description.type === 'offer') {
          await peerConnection.setLocalDescription();
          await sendWebRtcSignal('description', senderId, { description: peerConnection.localDescription });
        }
      }

      if (signalType === 'ice-candidate' && data.candidate) {
        try {
          await peerConnection.addIceCandidate(new RTCIceCandidate(data.candidate));
        } catch (error) {
          if (!ignoreOfferRef.current[senderId]) {
            throw error;
          }
        }
      }
    });

    nextConnection
      .start()
      .then(async () => {
        await nextConnection.invoke('JoinRoom', selectedRoomId);
        await publishMediaStatus();
      })
      .catch(() => toast.error('Live room connection failed'));

    connectionRef.current = nextConnection;
    setConnection(nextConnection);

    return () => {
      nextConnection.invoke('LeaveRoom', selectedRoomId).catch(() => {});
      nextConnection.off('ReceiveRoomMessage');
      nextConnection.off('WorkspaceUpdated');
      nextConnection.off('ParticipantOnline');
      nextConnection.off('ReceiveWebRtcSignal');
      nextConnection.stop();
      connectionRef.current = null;
      setConnection(null);
      Object.values(peerConnectionsRef.current).forEach((peerConnection) => peerConnection.close());
      peerConnectionsRef.current = {};
      setRemoteStreams({});
      setRemoteStreamVersions({});
      setRemoteMediaStatus({});
    };
  }, [canEnterRoom, createOfferForUser, createPeerConnection, publishMediaStatus, selectedRoomId, sendWebRtcSignal, user?.id]);

  useEffect(() => {
    return () => {
      localStreamRef.current.getTracks().forEach((track) => track.stop());
      Object.values(peerConnectionsRef.current).forEach((peerConnection) => peerConnection.close());
    };
  }, []);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [activeRoom?.messages]);

  const handleCreateRoom = async (event) => {
    event.preventDefault();
    try {
      setCreating(true);
      const payload = {
        ...form,
        maxMembers: Number(form.maxMembers),
        durationMinutes: Number(form.durationMinutes),
        scheduledAt: form.scheduledAt ? new Date(form.scheduledAt).toISOString() : null,
      };
      const res = await peerRoomsApi.createRoom(payload);
      setRooms((current) => [res.data, ...current]);
      setSelectedRoomId(res.data.id);
      setForm(emptyForm);
      setShowCreate(false);
      toast.success('Peer room created');
    } catch (err) {
      toast.error(err.message || 'Failed to create room');
    } finally {
      setCreating(false);
    }
  };

  const handleJoin = async (roomId) => {
    try {
      const res = await peerRoomsApi.requestToJoin(roomId);
      setRooms((current) => current.map((room) => (room.id === roomId ? res.data : room)));
      toast.success(res.data.currentUserStatus === 'Approved' ? 'Joined room' : 'Join request sent');
    } catch (err) {
      toast.error(err.message || 'Failed to join room');
    }
  };

  const handleParticipantDecision = async (participantId, approved) => {
    try {
      if (approved) {
        await peerRoomsApi.approveParticipant(activeRoom.id, participantId);
      } else {
        await peerRoomsApi.rejectParticipant(activeRoom.id, participantId);
      }
      await loadRoom(activeRoom.id);
      await loadRooms();
    } catch (err) {
      toast.error(err.message || 'Failed to update participant');
    }
  };

  const handleSendMessage = async (event) => {
    event.preventDefault();
    if (!message.trim()) return;
    try {
      if (connection) {
        await connection.invoke('SendRoomMessage', activeRoom.id, message.trim());
      }
      setMessage('');
    } catch (err) {
      toast.error(err.message || 'Failed to send message');
    }
  };

  const handleSaveWorkspace = async () => {
    try {
      await peerRoomsApi.saveWorkspace(activeRoom.id, { sharedNotes, whiteboardState });
      if (connection) {
        await connection.invoke('BroadcastWorkspaceUpdate', activeRoom.id, sharedNotes, whiteboardState);
      }
      toast.success('Workspace saved');
    } catch (err) {
      toast.error(err.message || 'Failed to save workspace');
    }
  };

  const handleCreateTask = async (event) => {
    event.preventDefault();
    if (!taskTitle.trim()) return;
    try {
      const res = await peerRoomsApi.createTask(activeRoom.id, { title: taskTitle.trim() });
      setActiveRoom((current) => ({ ...current, tasks: [res.data, ...(current.tasks || [])] }));
      setTaskTitle('');
    } catch (err) {
      toast.error(err.message || 'Failed to add task');
    }
  };

  const handleToggleTask = async (taskId) => {
    try {
      const res = await peerRoomsApi.toggleTask(activeRoom.id, taskId);
      setActiveRoom((current) => ({
        ...current,
        tasks: current.tasks.map((task) => (task.id === taskId ? res.data : task)),
      }));
    } catch (err) {
      toast.error(err.message || 'Failed to update task');
    }
  };

  if (loading) {
    return (
      <div className="flex min-h-[60vh] flex-col items-center justify-center text-gray-500">
        <Loader2 className="mb-4 h-10 w-10 animate-spin text-teal-500" />
        Loading peer rooms...
      </div>
    );
  }

  const focusedShareStream = focusedShare?.userId === user?.id
    ? localStream
    : remoteStreams[focusedShare?.userId];

  return (
    <div className="space-y-6">
      {focusedShare && (
        <SharedScreenOverlay
          stream={focusedShareStream}
          name={focusedShare.name}
          onClose={() => setFocusedShare(null)}
        />
      )}
      <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-gray-900">Peer Learning Rooms</h1>
          <p className="mt-1 text-gray-500">Study groups, hackathon spaces, shared chat, whiteboard notes, and task planning.</p>
        </div>
        <Button onClick={() => setShowCreate((value) => !value)} className="bg-teal-600 hover:bg-teal-700">
          <Plus className="mr-2 h-4 w-4" />
          New Room
        </Button>
      </div>

      {showCreate && (
        <Card className="border-gray-100 p-5 shadow-sm">
          <form onSubmit={handleCreateRoom} className="grid gap-4 lg:grid-cols-6">
            <div className="lg:col-span-2">
              <Label htmlFor="room-title">Title</Label>
              <Input
                id="room-title"
                value={form.title}
                onChange={(event) => setForm((current) => ({ ...current, title: event.target.value }))}
                placeholder="DSA sprint room"
                required
              />
            </div>
            <div className="lg:col-span-2">
              <Label htmlFor="room-type">Type</Label>
              <select
                id="room-type"
                value={form.collaborationType}
                onChange={(event) => setForm((current) => ({ ...current, collaborationType: event.target.value }))}
                className="h-10 w-full rounded-md border border-input bg-background px-3 text-sm"
              >
                <option>Study Group</option>
                <option>Hackathon</option>
                <option>Project Collaboration</option>
                <option>Interview Prep</option>
              </select>
            </div>
            <div>
              <Label htmlFor="room-members">Members</Label>
              <Input
                id="room-members"
                type="number"
                min="2"
                max="30"
                value={form.maxMembers}
                onChange={(event) => setForm((current) => ({ ...current, maxMembers: event.target.value }))}
              />
            </div>
            <div>
              <Label htmlFor="room-duration">Minutes</Label>
              <Input
                id="room-duration"
                type="number"
                min="15"
                max="480"
                value={form.durationMinutes}
                onChange={(event) => setForm((current) => ({ ...current, durationMinutes: event.target.value }))}
              />
            </div>
            <div className="lg:col-span-2">
              <Label htmlFor="room-schedule">Schedule</Label>
              <Input
                id="room-schedule"
                type="datetime-local"
                value={form.scheduledAt}
                onChange={(event) => setForm((current) => ({ ...current, scheduledAt: event.target.value }))}
              />
            </div>
            <div className="lg:col-span-4">
              <Label htmlFor="room-description">Description</Label>
              <Input
                id="room-description"
                value={form.description}
                onChange={(event) => setForm((current) => ({ ...current, description: event.target.value }))}
                placeholder="What will members work on together?"
                required
              />
            </div>
            <div className="flex items-end">
              <Button type="submit" disabled={creating} className="w-full bg-teal-600 hover:bg-teal-700">
                {creating ? <Loader2 className="mr-2 h-4 w-4 animate-spin" /> : <Plus className="mr-2 h-4 w-4" />}
                Create
              </Button>
            </div>
          </form>
        </Card>
      )}

      <div className="grid min-h-[680px] gap-6 xl:grid-cols-[360px_1fr]">
        <Card className="overflow-hidden border-gray-100 shadow-sm">
          <div className="border-b bg-gray-50/70 p-4">
            <div className="flex items-center gap-2 font-semibold text-gray-900">
              <Users className="h-5 w-5 text-teal-600" />
              Open Rooms
            </div>
          </div>
          <ScrollArea className="h-[610px]">
            <div className="divide-y divide-gray-100">
              {rooms.length === 0 ? (
                <div className="p-8 text-center text-sm text-gray-500">No rooms yet. Create the first one.</div>
              ) : (
                rooms.map((room) => (
                  <button
                    key={room.id}
                    onClick={() => setSelectedRoomId(room.id)}
                    className={`w-full p-4 text-left transition hover:bg-gray-50 ${
                      selectedRoomId === room.id ? 'bg-teal-50/70' : 'bg-white'
                    }`}
                  >
                    <div className="mb-2 flex items-start justify-between gap-3">
                      <h3 className="font-semibold text-gray-900">{room.title}</h3>
                      <Badge variant="secondary" className="shrink-0">{room.collaborationType}</Badge>
                    </div>
                    <p className="line-clamp-2 text-sm text-gray-500">{room.description}</p>
                    <div className="mt-3 flex flex-wrap items-center gap-3 text-xs text-gray-500">
                      <span className="inline-flex items-center gap-1">
                        <Users className="h-3.5 w-3.5" />
                        {room.approvedMembers}/{room.maxMembers}
                      </span>
                      <span className="inline-flex items-center gap-1">
                        <CalendarClock className="h-3.5 w-3.5" />
                        {formatSchedule(room.scheduledAt)}
                      </span>
                    </div>
                    <div className="mt-3 flex items-center justify-between">
                      <span className="text-xs text-gray-500">By {room.creatorName}</span>
                      {room.currentUserStatus === 'None' || room.currentUserStatus === 'Rejected' ? (
                        <Button
                          size="sm"
                          variant="outline"
                          onClick={(event) => {
                            event.stopPropagation();
                            handleJoin(room.id);
                          }}
                        >
                          Join
                        </Button>
                      ) : (
                        <Badge className={room.currentUserStatus === 'Approved' ? 'bg-teal-600' : 'bg-amber-500'}>
                          {room.currentUserStatus}
                        </Badge>
                      )}
                    </div>
                  </button>
                ))
              )}
            </div>
          </ScrollArea>
        </Card>

        <div className="min-w-0">
          {roomLoading ? (
            <Card className="flex h-full min-h-[620px] items-center justify-center border-gray-100">
              <Loader2 className="h-10 w-10 animate-spin text-teal-500" />
            </Card>
          ) : !activeRoom ? (
            <Card className="flex h-full min-h-[620px] items-center justify-center border-gray-100 text-gray-500">
              Select an approved room to open its workspace.
            </Card>
          ) : !canEnterRoom ? (
            <Card className="flex h-full min-h-[620px] flex-col items-center justify-center border-gray-100 p-8 text-center">
              <Users className="mb-4 h-12 w-12 text-amber-500" />
              <h2 className="text-xl font-semibold text-gray-900">{activeRoom.title}</h2>
              <p className="mt-2 max-w-md text-gray-500">Your request is waiting for the creator to approve before you can enter the room workspace.</p>
            </Card>
          ) : (
            <div className="grid gap-6 2xl:grid-cols-[1fr_360px]">
              <div className="space-y-6">
                <Card className="overflow-hidden border-gray-100 shadow-sm">
                  <div className="flex flex-col gap-4 border-b bg-gray-50/70 p-4 lg:flex-row lg:items-center lg:justify-between">
                    <div>
                      <div className="flex flex-wrap items-center gap-2">
                        <h2 className="text-xl font-semibold text-gray-900">{activeRoom.title}</h2>
                        <Badge className="bg-teal-600">{activeRoom.collaborationType}</Badge>
                      </div>
                      <p className="mt-1 text-sm text-gray-500">{activeRoom.description}</p>
                    </div>
                    <div className="flex gap-2">
                      <Button variant={micEnabled ? 'default' : 'outline'} size="icon" onClick={handleMicToggle} title={micEnabled ? 'Mute microphone' : 'Unmute microphone'}>
                        {micEnabled ? <Mic className="h-4 w-4" /> : <MicOff className="h-4 w-4" />}
                      </Button>
                      <Button variant={cameraEnabled ? 'default' : 'outline'} size="icon" onClick={handleCameraToggle} title={cameraEnabled ? 'Turn camera off' : 'Turn camera on'}>
                        {cameraEnabled ? <Video className="h-4 w-4" /> : <VideoOff className="h-4 w-4" />}
                      </Button>
                      <Button variant={screenSharing ? 'default' : 'outline'} size="icon" onClick={handleScreenShareToggle} title={screenSharing ? 'Stop screen sharing' : 'Share screen'}>
                        <MonitorUp className="h-4 w-4" />
                      </Button>
                    </div>
                  </div>

                  <div className="grid gap-3 bg-slate-950 p-4 md:grid-cols-2 xl:grid-cols-3">
                    <VideoTile
                      stream={localStream}
                      streamVersion={localStreamVersion}
                      name={user?.name || user?.fullName || 'You'}
                      muted
                      isLocal
                      micEnabled={micEnabled}
                      cameraEnabled={cameraEnabled}
                      screenSharing={screenSharing}
                      onExpand={screenSharing ? () => setFocusedShare({ userId: user?.id, name: user?.name || user?.fullName || 'You' }) : undefined}
                    />
                    {approvedParticipants
                      .filter((participant) => participant.userId !== user?.id)
                      .map((participant) => {
                        const status = remoteMediaStatus[participant.userId] || {};
                        return (
                          <VideoTile
                            key={participant.id}
                            stream={remoteStreams[participant.userId]}
                            streamVersion={remoteStreamVersions[participant.userId] || 0}
                            name={participant.name}
                            micEnabled={status.micEnabled}
                            cameraEnabled={status.cameraEnabled}
                            screenSharing={status.screenSharing}
                            onExpand={remoteStreams[participant.userId] ? () => setFocusedShare({ userId: participant.userId, name: participant.name }) : undefined}
                          />
                        );
                      })}
                  </div>
                </Card>

                <div className="grid gap-6 lg:grid-cols-2">
                  <Card className="border-gray-100 p-4 shadow-sm">
                    <div className="mb-3 flex items-center justify-between">
                      <div className="flex items-center gap-2 font-semibold text-gray-900">
                        <PenLine className="h-5 w-5 text-teal-600" />
                        Shared Whiteboard
                      </div>
                      <Button size="sm" onClick={handleSaveWorkspace}>Save</Button>
                    </div>
                    <Textarea
                      value={whiteboardState}
                      onChange={(event) => setWhiteboardState(event.target.value)}
                      className="min-h-64 resize-none"
                      placeholder="Sketch ideas as text, links, diagrams, or decisions..."
                    />
                  </Card>

                  <Card className="border-gray-100 p-4 shadow-sm">
                    <div className="mb-3 flex items-center justify-between">
                      <div className="flex items-center gap-2 font-semibold text-gray-900">
                        <ClipboardList className="h-5 w-5 text-teal-600" />
                        Room Notes
                      </div>
                      <Button size="sm" variant="outline" onClick={handleSaveWorkspace}>Sync</Button>
                    </div>
                    <Textarea
                      value={sharedNotes}
                      onChange={(event) => setSharedNotes(event.target.value)}
                      className="min-h-64 resize-none"
                      placeholder="Agenda, resources, decisions, next steps..."
                    />
                  </Card>
                </div>
              </div>

              <div className="space-y-6">
                <Card className="border-gray-100 shadow-sm">
                  <div className="border-b p-4">
                    <h3 className="font-semibold text-gray-900">Room Chat</h3>
                  </div>
                  <ScrollArea className="h-80 p-4">
                    <div className="space-y-3">
                      {(activeRoom.messages || []).map((item) => {
                        const isMe = item.senderId === user?.id;
                        return (
                          <div key={item.id} className={`flex ${isMe ? 'justify-end' : 'justify-start'}`}>
                            <div className={`max-w-[85%] rounded-lg px-3 py-2 text-sm ${isMe ? 'bg-teal-600 text-white' : 'bg-gray-100 text-gray-800'}`}>
                              <p className="mb-1 text-[11px] font-semibold opacity-80">{item.senderName}</p>
                              <p>{item.content}</p>
                            </div>
                          </div>
                        );
                      })}
                      <div ref={messagesEndRef} />
                    </div>
                  </ScrollArea>
                  <form onSubmit={handleSendMessage} className="flex gap-2 border-t p-3">
                    <Input value={message} onChange={(event) => setMessage(event.target.value)} placeholder="Message the room..." />
                    <Button type="submit" size="icon" className="shrink-0 bg-teal-600 hover:bg-teal-700">
                      <Send className="h-4 w-4" />
                    </Button>
                  </form>
                </Card>

                <Card className="border-gray-100 p-4 shadow-sm">
                  <h3 className="mb-3 font-semibold text-gray-900">Project Tasks</h3>
                  <form onSubmit={handleCreateTask} className="mb-4 flex gap-2">
                    <Input value={taskTitle} onChange={(event) => setTaskTitle(event.target.value)} placeholder="Add a task..." />
                    <Button type="submit" size="icon" className="shrink-0">
                      <Plus className="h-4 w-4" />
                    </Button>
                  </form>
                  <div className="space-y-2">
                    {(activeRoom.tasks || []).map((task) => (
                      <button
                        key={task.id}
                        onClick={() => handleToggleTask(task.id)}
                        className="flex w-full items-center gap-3 rounded-lg border border-gray-100 p-3 text-left transition hover:bg-gray-50"
                      >
                        {task.isCompleted ? <Check className="h-4 w-4 text-teal-600" /> : <Circle className="h-4 w-4 text-gray-400" />}
                        <span className={`text-sm ${task.isCompleted ? 'text-gray-400 line-through' : 'text-gray-800'}`}>{task.title}</span>
                      </button>
                    ))}
                  </div>
                </Card>

                {activeRoom.canManage && pendingParticipants.length > 0 && (
                  <Card className="border-gray-100 p-4 shadow-sm">
                    <h3 className="mb-3 font-semibold text-gray-900">Join Requests</h3>
                    <div className="space-y-2">
                      {pendingParticipants.map((participant) => (
                        <div key={participant.id} className="flex items-center justify-between rounded-lg border border-gray-100 p-3">
                          <span className="text-sm font-medium text-gray-800">{participant.name}</span>
                          <div className="flex gap-2">
                            <Button size="sm" variant="outline" onClick={() => handleParticipantDecision(participant.id, false)}>Reject</Button>
                            <Button size="sm" onClick={() => handleParticipantDecision(participant.id, true)}>Approve</Button>
                          </div>
                        </div>
                      ))}
                    </div>
                  </Card>
                )}
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default PeerRooms;
